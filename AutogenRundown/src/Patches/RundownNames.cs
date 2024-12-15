using AutogenRundown.DataBlocks;
using UnityEngine;

namespace AutogenRundown.Patches;

public class RundownNames
{
    /// <summary>
    /// This is used to override the vanilla game Rundown selection screen. It's tough but we must iterate through
    /// the list of values with the enumerator and pick the right index to select the right rundown. The index
    /// corresponding to each Rundown is:
    ///
    ///     0 -> Rundown 1
    ///     1 -> Rundown 7
    ///     2 -> Rundown 3
    ///     3 -> Rundown 2
    ///     4 -> Rundown 4
    ///     5 -> Rundown 5
    ///     6 -> Rundown 6
    ///     7 -> Rundown 8
    ///
    /// Not sure why it's in this order.
    /// </summary>
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

                rundown.m_rundownText.text = $"<size=70%><color=orange>DAILY</color><color=#444444>:</color> {rundownData.Title}</size>";
            }
            // Rundown ?
            else if (num == 4)
            {
                var rundownData = Bins.Rundowns.Find(Rundown.R4);

                rundown.m_rundownText.text = $"<size=70%><color=green>MONTHLY</color><color=#444444>:</color> {rundownData.Title}</size>";
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
