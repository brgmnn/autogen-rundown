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
        var num = 0;
        var enumerator = MainMenuGuiLayer.Current.PageRundownNew.m_rundownSelections.GetEnumerator();

        // MainMenuGuiLayer.Current.PageRundownNew.m_rundownTimerData = new RundownTimerData()
        // {
        //     ShowCountdownTimer = true,
        //     UTC_Target_Minute = 0,
        //     UTC_Target_Hour = 0,
        //     UTC_Target_Day = 1,
        //     UTC_Target_Month = 1,
        //     UTC_Target_Year = 2025
        // };

        while (enumerator.MoveNext())
        {
            // CM_RundownSelection rundown = enumerator.Current;
            var rundown = enumerator.Current;

            // Remove existing text for that rundown label
            if ((UnityEngine.Object)(object)rundown.m_altText != null)
            {
                UnityEngine.Object.Destroy(((Component)(object)rundown.m_altText).gameObject);
            }

            // Rundown 7
            ///
            /// Daily
            /// -> R1
            ///
            if (num == 0)
            {
                var rundownData = Bins.Rundowns.Find(Rundown.R_Daily);

                rundown.m_rundownText.text = $"<size=70%><color=orange>DAILY</color><color=#444444>:</color> {rundownData?.Title}</size>";
            }
            ///
            /// Weekly
            /// -> R3
            ///
            else if (num == 2)
            {
                var rundownData = Bins.Rundowns.Find(Rundown.R_Weekly);

                rundown.m_rundownText.text = $"<size=70%><color=green>WEEK #{Generator.WeekNumber}</color><color=#444444>:</color> {rundownData?.Title}</size>";
            }
            // Rundown 4
            ///
            /// Monthly
            /// -> R7
            ///
            else if (num == 1)
            {
                var rundownData = Bins.Rundowns.Find(Rundown.R_Monthly);

                rundown.m_rundownText.text = $"<size=70%><color=#58fcee>MONTHLY</color><color=#444444>:</color> {rundownData?.Title}</size>";
            }

            // Seasonal
            else if (num == 7)
            {
                var rundownData = Bins.Rundowns.Find(Rundown.R_Seasonal);

                rundown.m_rundownText.text = $"<size=70%><color=#ff3311>SEASONAL</color><color=#444444>:</color> FALL '25</size>";
            }

            num++;
        }
    }
}
