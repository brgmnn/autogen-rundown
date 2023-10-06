//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using MTFO.Ext.PartialData.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutogenRundown.JsonConverters;
using GameData;
using Localization;

namespace AutogenRundown.DataBlockTypes
{
    internal class DataBlockTypeWrapper<T> : IDataBlockType where T : GameDataBlockBase<T>
    {
        public Action OnForceChange;
        public string FullName { get; private set; }
        public string ShortenName { get; private set; }

        public DataBlockTypeWrapper()
        {
            FullName = typeof(T).Name.Trim();
            ShortenName = FullName.Replace("DataBlock", "");
        }

        public void OnChanged()
        {
            OnForceChange?.Invoke();
        }

        public void AddBlock(T block)
        {
            var existingBlock = GameDataBlockBase<T>.GetBlock(block.persistentID);
            if (existingBlock != null)
            {
                CopyProperties(block, existingBlock);
                //Logger.Warning($"Replaced Block: {existingBlock.persistentID}, {existingBlock.name}");
                return;
            }
            GameDataBlockBase<T>.AddBlock(block, -1);
            //Logger.Log($"Added Block: {block.persistentID}, {block.name}");
        }

        public void AddJsonBlock(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
                switch (doc.RootElement.ValueKind)
                {
                    case JsonValueKind.Array:
                        T[] blocks = (T[])JSON.Deserialize(json, typeof(T).MakeArrayType());
                        //List<T> blocks = JsonConvert.DeserializeObject<List<T>>(json);
                        foreach (var b in blocks)
                        {
                            AddBlock(b);
                        }
                        break;

                    case JsonValueKind.Object:
                        T block = (T)JSON.Deserialize(json, typeof(T));
                        //T block = JsonConvert.DeserializeObject<T>(json);
                        AddBlock(block);
                        break;
                }
            }
            catch (Exception e)
            {
                //Logger.Error($"Error While Adding Block: {e}");
            }
        }

        public void DoSaveToDisk(string fullPath)
        {
            var oldPath = GameDataBlockBase<T>.m_filePathFull;
            GameDataBlockBase<T>.m_filePathFull = fullPath;
            GameDataBlockBase<T>.DoSaveToDisk(false, false);
            GameDataBlockBase<T>.m_filePathFull = oldPath;
        }

        private static object CopyProperties(object source, object target)
        {
            foreach (var sourceProp in source.GetType().GetProperties())
            {
                var sourceType = sourceProp.PropertyType;

                var targetProp = target.GetType().GetProperties().FirstOrDefault(x => x.Name == sourceProp.Name && x.PropertyType == sourceProp.PropertyType && x.CanWrite);
                if (targetProp != null)
                {
                    if (sourceProp.Name.Contains("_k__BackingField"))
                    {
                        continue;
                    }

                    if (sourceType == typeof(IntPtr))
                    {
                        //Logger.Error("Pointer has detected on CopyProperties!!!!");
                        continue;
                    }

                    targetProp.SetValue(target, sourceProp.GetValue(source));
                }
            }
            return target;
        }

        public string GetShortName()
        {
            return ShortenName;
        }

        public string GetFullName()
        {
            return FullName;
        }

        public void RegisterOnChangeEvent(Action onChanged)
        {
            OnForceChange += onChanged;
        }
    }
}
