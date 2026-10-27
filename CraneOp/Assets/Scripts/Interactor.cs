/*
    This script is used to handle interactions in the game, specifically for module selection.
    It can be used to create other interaction functionalities in the future as needed.
*/
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
