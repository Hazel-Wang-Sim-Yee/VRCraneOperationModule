using UnityEngine;

public class LoadHookSettings : MonoBehaviour
{
    GameObject loadPos;

    void Start()
    {
        loadPos = GameObject.Find("loadPos");
        if (loadPos == null)
        {
            Debug.LogError("loadPos GameObject not found in the scene. Please ensure it exists.");
        }
    }

    public void LoadLoaded()
    {
        loadPos.transform.position = GameObject.Find("load").transform.position;
        Events.Instance.LoadLoaded();
    }

    public void LoadUnloaded()
    {
        loadPos.transform.position = GameObject.Find("hook").transform.position;
        Events.Instance.LoadUnloaded();
    }
}
