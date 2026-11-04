/*
    This script is used to handle interactions in the game, specifically for module selection.
    It can be used to create other interaction functionalities in the future as needed.
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class Interactor : MonoBehaviour
{
    Coroutine loadingCoroutine;

    [Header("Module Selection Buttons")]
    [SerializeField] TextMeshProUGUI[] moduleButtonTexts; // Array to hold the TextMeshProUGUI components of the module selection buttons

    void Start()
    {
        Events.Instance.onSequenceComplete += LoadNextModule; // Subscribe to the sequence complete event to load the next module
        // Initialize the module selection buttons with the names of the modules in the build settings
        for (int i = 0; i < moduleButtonTexts.Length; i++)
        {
            if (i < SceneManager.sceneCountInBuildSettings)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                moduleButtonTexts[i].text = $"Module {i+1}: {sceneName}"; // Set the button text to the module name
            }
            else
            {
                moduleButtonTexts[i].text = "N/A"; // If there are more buttons than modules, set the text to "N/A"
                Debug.LogWarning($"No module found for button index {i}. Please check the build settings.");
            }
        }
    }

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
            loadingCoroutine = StartCoroutine(loadingCoroutineMethod(moduleIndex, 3)); // Start the coroutine to load the new scene with a 3-second delay
        }
        else
        {
            Debug.LogError("Invalid module index: " + moduleIndex);
        }
    }

    /// <summary>
    /// Loads the next module in the build settings if available.
    /// If there are no more modules to load, it will return to the first module and provide feedback to the player.
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
            UI.instance.UpdateFeedbackText("No more modules to load. Returning to the first module.");
            ModuleSelection(0);
        }
    }

    /// <summary>
    /// Coroutine to handle the loading of a new scene based on the module index.
    /// </summary>
    /// <param name="moduleIndex"></param>
    /// <returns></returns>
    IEnumerator loadingCoroutineMethod(int moduleIndex, int delay)
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(moduleIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name not found for module index: " + moduleIndex);
            yield break;
        }
        UI.instance.UpdateFeedbackText("Loading module: " + sceneName);
        yield return new WaitForSeconds(delay); // Wait for the specified delay before loading the scene
        SceneManager.LoadScene(moduleIndex);
    }
}
