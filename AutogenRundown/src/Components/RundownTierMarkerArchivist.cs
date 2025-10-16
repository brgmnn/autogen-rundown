using GTFO.API;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

namespace AutogenRundown.Components;

internal class RundownTierMarkerArchivist : MonoBehaviour
{
    internal CM_RundownTierMarker m_tierMarker;

    private CM_ExpeditionSectorIcon m_completeWithNoBoosterIcon = null;
    private SpriteRenderer m_icon;
    private SpriteRenderer m_bg;
    private TextMeshPro m_title;
    private TextMeshPro m_rightSideText;

    private ArchivistIconWrapper Wrapper;

    private static byte[] spriteData;
    private static Texture2D texture;
    private SpriteRenderer m_sprite;

    private static GameObject? Icon { get; set; }

    internal static void OnAssetBundlesLoaded()
    {
        Icon = AssetAPI.GetLoadedAsset<GameObject>("Assets/Misc/CM_ExpeditionSectorIcon.prefab");

        var dir = Path.Combine(Paths.PluginPath, Plugin.Name);
        var path = Path.Combine(dir, "dlock.png");

        if (!File.Exists(path))
        {
            Plugin.Logger.LogError($"File not found: {path}");
            return;
        }

        spriteData = File.ReadAllBytes(path);

        // Decode into RGBA32 texture (works fine for JPG)
        texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!ImageConversion.LoadImage(texture, spriteData, markNonReadable: false))
            Plugin.Logger.LogError("Failed to decode image.");
    }

    internal void Setup()
    {
        if (m_tierMarker == null)
        {
            Plugin.Logger.LogError("Assign the page instance before setup");
            return;
        }

        if (m_completeWithNoBoosterIcon != null)
        {
            Plugin.Logger.LogError($"longicon is not null");
            return;
        }

        if (Icon == null)
        {
            AssetAPI.OnAssetBundlesLoaded += () => {
                LoadAsset();
                AssetAPI.OnAssetBundlesLoaded -= LoadAsset;
            };
        }
        else
        {
            LoadAsset();
        }
    }

    private void LoadAsset()
    {
        if (Icon == null)
        {
            Plugin.Logger.LogError("RundownTierMarkerArchivist.Setup: cannot instantiate NoBooster icon...");
            return;
        }

        Plugin.Logger.LogError("RundownTierMarkerArchivist.Setup: setting it up");

        m_completeWithNoBoosterIcon = GOUtil.SpawnChildAndGetComp<CM_ExpeditionSectorIcon>(
            Icon, m_tierMarker.m_sectorIconAlign_main);

        Wrapper = new(m_completeWithNoBoosterIcon.gameObject);

        m_bg = Wrapper.BGGO.GetComponent<SpriteRenderer>();
        m_icon = Wrapper.IconGO.GetComponent<SpriteRenderer>();

        m_title = Instantiate(m_tierMarker.m_sectorIconSummaryMain.m_title);
        m_title.transform.SetParent(Wrapper.ObjectiveIcon.transform, false);
        m_rightSideText = Instantiate(m_tierMarker.m_sectorIconSummaryMain.m_rightSideText);
        m_rightSideText.transform.SetParent(Wrapper.RightSideText.transform, false);

        m_completeWithNoBoosterIcon.m_title = m_title;
        m_completeWithNoBoosterIcon.m_rightSideText = m_rightSideText;

        SetupNoBoosterUsedIcon(true);

        var scale = 0.16f;
        var localScale = new Vector3(scale, scale, scale);
        var num = scale / 0.16f;

        m_completeWithNoBoosterIcon.transform.localScale = localScale;
        // var diff = m_tierMarker.m_sectorIconSummarySecondary.GetPosition() - m_tierMarker.m_sectorIconSummaryMain.GetPosition();

        m_completeWithNoBoosterIcon.SetPosition(new Vector2 { x = 0f, y = 150f });
        // m_completeWithNoBoosterIcon.SetVisible(false);
    }

    internal void SetVisible(bool visible) => m_completeWithNoBoosterIcon.SetVisible(visible);

    internal void SetSectorIconText(string text)
    {
        m_completeWithNoBoosterIcon.SetVisible(true);
        m_completeWithNoBoosterIcon.SetRightSideText(text);
    }

    private void SetupNoBoosterUsedIcon(bool boosterUnused)
    {
        var icon = m_completeWithNoBoosterIcon;
        icon.m_isFinishedAll = true;
        icon.SetupIcon(icon.m_iconMainSkull, icon.m_iconMainBG, false);
        icon.SetupIcon(icon.m_iconSecondarySkull, icon.m_iconSecondaryBG, false);
        icon.SetupIcon(icon.m_iconThirdSkull, icon.m_iconThirdBG, false);
        icon.SetupIcon(icon.m_iconFinishedAllSkull, icon.m_iconFinishedAllBG, false, false, 0.5f);
        //icon.SetupIcon(m_icon, m_bg, true, boosterUnused, 1.0f, 1.0f);
        var cIcon = m_icon.color;
        var cBg = m_bg.color;
        m_icon.color = new(cIcon.r, cIcon.g, cIcon.b, boosterUnused ? 1.0f : 0.4f);
        m_bg.color = new(cBg.r, cBg.g, cBg.b, boosterUnused ? 1.0f : 0.3f);
        m_title.alpha = (boosterUnused ? 1f : 0.2f);

        icon.m_titleVisible = true;
        icon.m_isCleared = boosterUnused;

        m_bg.gameObject.SetActive(false);

        // Create sprite
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f // pixels per unit
        );

        // Assign
        m_icon.sprite = sprite;
        m_icon.color = Color.white; // ensure it's not tinted


        // blink in sound control
        if (boosterUnused)
        {
            icon.m_isFinishedAll = true;
        }
        else
        {
            icon.m_isFinishedAll = false;
            icon.m_type = LevelGeneration.LG_LayerType.MainLayer;
        }

        icon.m_rightSideText.gameObject.SetActive(false);

        icon.m_title.SetText("Log Archivist");


        //sector_icon.m_title.fontSize = m_page.m_sectorIconMain.m_title.fontSize;
        icon.m_title.gameObject.SetActive(true);
    }

    static RundownTierMarkerArchivist()
    {
        ClassInjector.RegisterTypeInIl2Cpp<RundownTierMarkerArchivist>();
    }
}
