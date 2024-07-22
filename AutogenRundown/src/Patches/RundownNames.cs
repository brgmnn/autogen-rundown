using AutogenRundown.DataBlocks;
using UnityEngine;

namespace AutogenRundown.Patches
{
    public class RundownNames
    {
        public static void OnTitleDataUpdated()
        {
            int num = 0;
            var enumerator = MainMenuGuiLayer.Current.PageRundownNew.m_rundownSelections.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var rundown = enumerator.Current;

                // Remove existing text for that rundown label
                if ((UnityEngine.Object)(object)rundown.m_altText != null)
                {
                    UnityEngine.Object.Destroy(((Component)(object)rundown.m_altText).gameObject);
                }

                // Rundown 7
                if (num == 1)
                {
                    var rundownData = Bins.Rundowns.Find(Rundown.R7);
                    var name = rundownData?.DisplaySeed;

                    if (name == "")
                        name = $"<color=orange>{Generator.Seed}</color>";

                    rundown.m_rundownText.text = $"<size=60%><color=green>RND</color><color=#444444>://</color></size>R{name}";
                }
                else
                {
                    // Disable other rundowns
                    rundown.SetButtonEnabled(false);
                    rundown.SetIsUsed(false);
                    rundown.m_rundownText.text = "";
                }

                num++;
            }
        }
    }
}
