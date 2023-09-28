using CellMenu;
using GameData;
//using MTFO.API;
//using MTFO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using UnityEngine.UI;
using UniverseLib;

namespace MyFirstPlugin
{
    public interface IHotManager
    {
        public void OnHotReload(int id);
    }

    class HotGameDataManager : IHotManager
    {
        public void OnHotReload(int id)
        {
            //GameManager.SetRundown();
            //GameSetupDataBlock.

            Plugin.Logger.LogInfo("Reinitialized GameData");
        }
    }

    public class HotReloader : MonoBehaviour
    {
        public HotReloader(IntPtr intPtr) : base(intPtr) { }

        /// <summary>
        /// Create a simple UI object with a RectTransform. <paramref name="parent"/> can be null.
        /// </summary>
        public static GameObject CreateUIObject(string name, GameObject parent, Vector2 sizeDelta = default)
        {
            GameObject obj = new(name)
            {
                layer = 5,
                hideFlags = HideFlags.HideAndDontSave,
            };

            if (parent)
                obj.transform.SetParent(parent.transform, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;
            return obj;
        }

        public void Awake()
        {
            //gameObject.transform.localPosition = buttonPosition;
            button = gameObject.GetComponent<CM_Item>();
            button.SetText(buttonLabel);
            button.m_spriteColorOrg = Color.white;
            AddOnReloadListener(new HotGameDataManager());
            //AddOnReloadListener(new HotRundownManager());
            //AddOnReloadListener(new HotGearManager());
            button.add_OnBtnPressCallback(new Action<int>((_) =>
            {
                //MTFOHotReloadAPI.HotReloaded();
            }));
        }

        /// <summary>
        /// Adds callback to a button and manager to a dictionary if it doesn't exist already
        /// </summary>
        public void AddOnReloadListener(IHotManager manager)
        {
            if (!managers.Contains(manager))
            {
                button.add_OnBtnPressCallback((Action<int>)manager.OnHotReload);
                managers.Add(manager);
            }
        }

        /// <summary>
        /// Removes callback from a button and manager from a dictionary if it does exist already
        /// </summary>
        public void RemoveOnReloadListener(IHotManager manager)
        {
            if (managers.Contains(manager))
            {
                button.remove_OnBtnPressCallback((Action<int>)manager.OnHotReload);
                managers.Remove(manager);
            }
        }

        /// <summary>
        /// Create a HotReloader instance if it doesn't exist and assigns it to a singleton
        /// </summary>
        public static void Setup()
        {
            if (Current != null || MainMenuGuiLayer.Current.PageRundownNew == null) return;

            GameObject button = Instantiate(
                original: MainMenuGuiLayer.Current.PageRundownNew.m_tutorialButton.gameObject,
                parent: MainMenuGuiLayer.Current.PageRundownNew.m_tutorialButton.transform.parent,
                worldPositionStays: false);

            // Remove old training button
            MainMenuGuiLayer.Current.PageRundownNew.m_tutorialButton.gameObject.transform.localPosition = new(-100000, 0, 0);

            button.name = "Rundown Config";
            Current = button.AddComponent<HotReloader>();
            button.SetActive(true);

            //GameManager.SetRundown();
        }


        public static HotReloader Current;
        private CM_Item button;
        private readonly string buttonLabel = "RUNDOWN CONFIG";
        //private readonly Vector3 buttonPosition = new(0, 0, 0);
        private readonly List<IHotManager> managers = new();
    }
}
