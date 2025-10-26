using UnityEngine;

namespace AutogenRundown.Components;

public class ArchivistIconWrapper
{
    public GameObject GameObject { get; }

    public GameObject ObjectiveIcon => GameObject.transform.GetChild(0).gameObject;

    public GameObject BGHolder => ObjectiveIcon.transform.GetChild(2).gameObject;

    public GameObject SkullHolder => ObjectiveIcon.transform.GetChild(3).gameObject;

    public GameObject BGGO => BGHolder.transform.GetChild(4).gameObject;

    public GameObject IconGO => SkullHolder.transform.GetChild(4).gameObject;

    public GameObject TitleGO => ObjectiveIcon.transform.GetChild(1).gameObject;

    public GameObject RightSideText => GameObject.transform.GetChild(2).gameObject;

    public ArchivistIconWrapper(GameObject icon)
    {
        GameObject = icon;
    }

    public void Destroy()
    {
        if (GameObject != null)
        {
            UnityEngine.Object.Destroy(GameObject);
            // GameObject.Destroy(GameObject);
        }
    }
}
