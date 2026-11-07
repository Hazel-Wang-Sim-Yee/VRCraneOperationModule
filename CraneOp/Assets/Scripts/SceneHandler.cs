/*
    This script manages scene transitions and module selection in the VR Crane Operation Module.
    It handles loading new scenes based on user input, updates the UI with the current module information, and manages the sequence of modules in the build settings.
    The script also ensures that the player is informed about the loading process through UI feedback.
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SceneHandler : MonoBehaviour
{
    Coroutine loadingCoroutine;
    public int currentModuleIndex;

    [Header("Module Selection Buttons")]
    [SerializeField] TextMeshProUGUI[] moduleButtonTexts; // Array to hold the TextMeshProUGUI components of the module selection buttons

    void Start()
    {
        StopAllCoroutines(); // Stop any existing coroutines to prevent conflicts

        if (UI.instance.loadingText != null)
        {
            UI.instance.loadingText.text = "Loading Module: " + SceneManager.GetActiveScene().name;
        }
        else
        {
            Debug.LogWarning("UI instance has failed to find loading text.");
        }

        // Subscribe to the sequence complete event to load the next module if not in crane modules
        if (!SceneManager.GetActiveScene().name.Contains("Crane"))
        {
            Events.Instance.onSequenceComplete += LoadNextModule;
        }

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

        currentModuleIndex = SceneManager.GetActiveScene().buildIndex; // Store the current module index for reference
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
    /// If there are no more modules to load, it will return to the first module and provide feedback to the player.
    /// </summary>
    public void LoadNextModule()
    {
        int nextModuleIndex = currentModuleIndex + 1;
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
    IEnumerator loadingCoroutineMethod(int moduleIndex)
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(moduleIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name not found for module index: " + moduleIndex);
            yield break;
        }
        if (UI.instance.loadingAnimator != null && UI.instance.loadingText != null)
        {
            UI.instance.loadingAnimator.SetTrigger("Teleporting");
            UI.instance.loadingText.text = "Loading Module: " + sceneName;
            yield return new WaitForSeconds(UI.instance.loadingAnimator.GetCurrentAnimatorStateInfo(0).length); // Wait for the animation to finish
            SceneManager.LoadScene(moduleIndex);
        }
        else
        {
            Debug.LogWarning("Loading animator is not assigned in the inspector.");
            loadingCoroutine = null;
        }
    }
}
