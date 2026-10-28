/*
    This script is used to handle interactions in the game, specifically for module selection.
    It can be used to create other interaction functionalities in the future as needed.
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Interactor : MonoBehaviour
{
    Coroutine loadingCoroutine;

    /// <summary>
    /// Handles the module selection based on the provided module index.
    /// It checks if the module index is valid and if the player is already in the selected module.
    /// If not, it starts a coroutine to load the new scene corresponding to the module index.
    /// </summary>
    /// <param name="moduleIndex"></param>
    public void ModuleSelection(int moduleIndex)
    {
        if (moduleIndex < SceneManager.sceneCountInBuildSettings)
        {
            if (SceneManager.GetActiveScene().buildIndex == moduleIndex)
            {
                UI.instance.UpdateFeedbackText("Already in the selected module: " + SceneManager.GetSceneByBuildIndex(moduleIndex).name);
                return;
            }
            if (loadingCoroutine != null)
            {
                StopCoroutine(loadingCoroutine);
            }
            loadingCoroutine = StartCoroutine(loadingCoroutineMethod(moduleIndex));
        }
        else
        {
            Debug.LogError("Invalid module index: " + moduleIndex);
        }
    }

    /// <summary>
    /// Loads the next module in the build settings if available.
    /// If there are no more modules to load, it provides feedback to the user.
    /// </summary>
    public void LoadNextModule()
    {
        int nextModuleIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextModuleIndex < SceneManager.sceneCountInBuildSettings)
        {
            ModuleSelection(nextModuleIndex);
        }
        else
        {
            UI.instance.UpdateFeedbackText("No more modules to load.");
        }
    }

    /// <summary>
    /// Coroutine to handle the loading of a new scene based on the module index.
    /// </summary>
    /// <param name="moduleIndex"></param>
    /// <returns></returns>
    IEnumerator loadingCoroutineMethod(int moduleIndex)
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(moduleIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name not found for module index: " + moduleIndex);
            yield break;
        }
        UI.instance.UpdateFeedbackText("Loading module: " + sceneName);
        yield return new WaitForSeconds(1f); // Wait for 1 second before loading the scene
        SceneManager.LoadScene(moduleIndex);
    }
}
