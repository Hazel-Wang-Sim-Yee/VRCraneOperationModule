using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactor : MonoBehaviour
{
    public void ModuleSelection(int moduleIndex)
    {
        if (moduleIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(moduleIndex);
        }
        else
        {
            Debug.LogError("Invalid module index: " + moduleIndex);
        }
    }
}
