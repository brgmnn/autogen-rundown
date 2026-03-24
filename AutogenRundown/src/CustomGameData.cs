using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown;

/// <summary>
/// Applies user custom GameData overrides from BepInEx/GameData-Custom/.
///
/// Files in GameData-Custom/ mirror the GameData/{revision}/ directory structure.
/// JSON files are deep-merged into the generated output so users only need to specify
/// the properties they want to change.
///
/// Array merge modes:
///   - persistentID: if patch array elements have "persistentID", match by that value
///   - __index: if patch array elements have "__index", merge into target at that position
///   - replace: otherwise, replace the entire target array
/// </summary>
public static class CustomGameData
{
    private static string CustomDir =>
        Path.Combine(Paths.BepInExRootPath, "GameData-Custom");

    private static string GameDataDir =>
        Path.Combine(Paths.BepInExRootPath, "GameData", $"{CellBuildData.GetRevision()}");

    /// <summary>
    /// Entry point — scans GameData-Custom/ and merges or copies each file into
    /// the generated GameData/{revision}/ directory.
    /// </summary>
    public static void Apply()
    {
        if (!Directory.Exists(CustomDir))
            return;

        Plugin.Logger.LogInfo($"Applying custom GameData overrides from: {CustomDir}");

        var files = Directory.GetFiles(CustomDir, "*", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            Plugin.Logger.LogInfo("GameData-Custom/ is empty, nothing to apply");
            return;
        }

        foreach (var customPath in files)
        {
            var relativePath = Path.GetRelativePath(CustomDir, customPath);
            var targetPath = Path.Combine(GameDataDir, relativePath);

            // Ensure target directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            if (Path.GetExtension(customPath).Equals(".json", StringComparison.OrdinalIgnoreCase)
                && File.Exists(targetPath))
            {
                // Both are JSON and target exists — deep merge
                MergeFile(targetPath, customPath);
                Plugin.Logger.LogInfo($"GameData-Custom: Merged {relativePath}");
            }
            else
            {
                // Non-JSON or target doesn't exist — copy as-is
                File.Copy(customPath, targetPath, overwrite: true);
                Plugin.Logger.LogInfo($"GameData-Custom: Copied {relativePath}");
            }
        }
    }

    /// <summary>
    /// Reads both files as JTokens, deep-merges, and writes the result back.
    /// </summary>
    private static void MergeFile(string generatedPath, string customPath)
    {
        try
        {
            var generatedText = File.ReadAllText(generatedPath);
            var customText = File.ReadAllText(customPath);

            var generated = JToken.Parse(generatedText);
            var custom = JToken.Parse(customText);

            var merged = DeepMerge(generated, custom);

            RecalculateLastPersistentId(merged);

            File.WriteAllText(generatedPath, merged.ToString(Formatting.Indented));
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"GameData-Custom: Failed to merge {Path.GetFileName(customPath)}: {ex.Message}");
        }
    }

    /// <summary>
    /// For datablock files (those with a "Blocks" array and "LastPersistentID"),
    /// recalculates LastPersistentID to be the max of all block persistentIDs.
    /// </summary>
    private static void RecalculateLastPersistentId(JToken merged)
    {
        if (merged is not JObject root)
            return;

        if (root["Blocks"] is not JArray blocks || root["LastPersistentID"] == null)
            return;

        uint max = root["LastPersistentID"]!.Value<uint>();

        foreach (var block in blocks)
        {
            if (block is JObject obj && obj["persistentID"] != null)
            {
                var pid = obj["persistentID"]!.Value<uint>();
                if (pid > max)
                    max = pid;
            }
        }

        root["LastPersistentID"] = max;
    }

    /// <summary>
    /// Recursively deep-merges patch into target.
    ///
    /// - Objects: merge properties recursively
    /// - Arrays: detect merge mode from patch elements (persistentID, __index, or replace)
    /// - Scalars / type mismatch: patch wins
    /// </summary>
    internal static JToken DeepMerge(JToken target, JToken patch)
    {
        // Object + Object: recursive property merge
        if (patch is JObject patchObj && target is JObject targetObj)
        {
            foreach (var property in patchObj.Properties())
            {
                var existing = targetObj[property.Name];

                if (existing != null)
                    targetObj[property.Name] = DeepMerge(existing, property.Value);
                else
                    targetObj[property.Name] = property.Value.DeepClone();
            }

            return targetObj;
        }

        // Array + Array: detect merge mode
        if (patch is JArray patchArr && target is JArray targetArr)
        {
            MergeArray(targetArr, patchArr);
            return targetArr;
        }

        // Scalar or type mismatch: patch replaces target
        return patch.DeepClone();
    }

    /// <summary>
    /// Merges a patch array into the target array using one of four modes.
    /// </summary>
    private static void MergeArray(JArray target, JArray patch)
    {
        // Detect merge mode from patch elements
        var hasPersistentId = false;
        var hasIndex = false;
        var hasExisting = false;

        foreach (var item in patch)
        {
            if (item is JObject obj)
            {
                if (obj.ContainsKey("persistentID"))
                    hasPersistentId = true;
                if (obj.ContainsKey("__index"))
                    hasIndex = true;
            }
            else if (item.Type == JTokenType.String && item.Value<string>() == "__existing")
            {
                hasExisting = true;
            }
        }

        if (hasPersistentId)
            MergeArray_ByPersistentId(target, patch);
        else if (hasIndex)
            MergeArray_ByIndex(target, patch);
        else if (hasExisting)
            MergeArray_Splice(target, patch);
        else
            MergeArray_Replace(target, patch);
    }

    /// <summary>
    /// Match patch elements to target elements by persistentID.
    /// Unmatched elements are appended as new entries.
    /// </summary>
    private static void MergeArray_ByPersistentId(JArray target, JArray patch)
    {
        foreach (var patchItem in patch)
        {
            if (patchItem is not JObject patchObj)
                continue;

            var pid = patchObj["persistentID"];
            if (pid == null)
                continue;

            // Find matching target element
            JObject? targetObj = null;
            foreach (var targetItem in target)
            {
                if (targetItem is JObject tObj && JToken.DeepEquals(tObj["persistentID"], pid))
                {
                    targetObj = tObj;
                    break;
                }
            }

            if (targetObj != null)
            {
                // Merge into existing block
                DeepMerge(targetObj, patchObj);
            }
            else
            {
                // New block — append
                target.Add(patchObj.DeepClone());
                Plugin.Logger.LogDebug($"GameData-Custom: Added new block with persistentID={pid}");
            }
        }
    }

    /// <summary>
    /// Match patch elements to target elements by __index position.
    /// The __index property is stripped from the output.
    /// </summary>
    private static void MergeArray_ByIndex(JArray target, JArray patch)
    {
        foreach (var patchItem in patch)
        {
            if (patchItem is not JObject patchObj)
                continue;

            var indexToken = patchObj["__index"];
            if (indexToken == null)
                continue;

            var index = indexToken.Value<int>();

            // Create a copy without __index
            var patchCopy = (JObject)patchObj.DeepClone();
            patchCopy.Remove("__index");

            if (index >= 0 && index < target.Count)
            {
                if (target[index] is JObject targetObj)
                    DeepMerge(targetObj, patchCopy);
                else
                    target[index] = patchCopy;
            }
            else
            {
                Plugin.Logger.LogWarning(
                    $"GameData-Custom: __index={index} is out of bounds (array has {target.Count} elements)");
            }
        }
    }

    /// <summary>
    /// Splice mode: the patch array contains an "__existing" string marker that expands
    /// to all original target elements. Items before it are prepended, items after are appended.
    /// </summary>
    private static void MergeArray_Splice(JArray target, JArray patch)
    {
        var result = new JArray();

        foreach (var item in patch)
        {
            if (item.Type == JTokenType.String && item.Value<string>() == "__existing")
            {
                // Expand original array contents at this position
                foreach (var existing in target)
                    result.Add(existing.DeepClone());
            }
            else
            {
                result.Add(item.DeepClone());
            }
        }

        target.Clear();
        foreach (var item in result)
            target.Add(item);
    }

    /// <summary>
    /// Replace the entire target array with the patch array.
    /// </summary>
    private static void MergeArray_Replace(JArray target, JArray patch)
    {
        target.Clear();
        foreach (var item in patch)
            target.Add(item.DeepClone());
    }
}
