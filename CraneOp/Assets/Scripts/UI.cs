/*
    This script manages UI elements in the game, including loading screens, feedback messages, and score updates.
    It provides methods to update the UI based on game events and ensures that the UI elements are properly initialized and displayed to the player.
    The script also handles the rotation of world-space canvases to face the camera, enhancing the player's experience in the VR environment.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)] // Ensure this script runs before other scripts that may depend on it
public class UI : MonoBehaviour
{
    [Header("Loading Screen (auto-assigned by name if not assigned)")]
    public Animator loadingAnimator;
    public TextMeshProUGUI loadingText;

    [Header("Feedback")]
    [SerializeField] TextMeshProUGUI feedbackText;
    Coroutine clearFeedbackCoroutine;

    [Header("Crane modules Canvas")]
    [Tooltip("This is the canvas that contains the controller buttons.")]
    public Image controllerCanvas;
    public Image finalScoreCanvas;

    [Header("Singleton")]
    public static UI instance;

    [Header("Canvas Settings")]
    [Tooltip("Drag all the world-space canvases you want to rotate here.")]
    [SerializeField] Canvas[] canvasesToRotate;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        if (loadingAnimator == null)
        {
            loadingAnimator = GameObject.Find("LoadingUI").GetComponent<Animator>();
        }
        if (loadingText == null)
        {
            loadingText = GameObject.Find("LoadingText").GetComponent<TextMeshProUGUI>();
        }

        feedbackText.text = "";
        
        // Subscribe based on current scene to avoid unnecessary subscriptions in non-relevant scenes
        if (SceneManager.GetActiveScene().name.Contains("Crane"))
        {
            // Ensure that the controller canvas is assigned in the inspector for crane modules
            if (controllerCanvas == null || finalScoreCanvas == null)
            {
                Debug.LogWarning("Ensure that the Controller Canvas and Final Score Canvas are assigned if in crane modules.");
            } else {
                controllerCanvas.gameObject.SetActive(false);
                finalScoreCanvas.gameObject.SetActive(false);
            }

            Events.Instance.onLoadLoaded += ActivateControllerCanvas;
            Events.Instance.onLoadUnloaded += DeactivateControllerCanvas;
            Events.Instance.onSequenceComplete += DeactivateControllerCanvas; // Deactivate the controller canvas when the sequence is complete
        }
    }

    /// <summary>
    /// Updates the feedback text with a specified message and clears it after a delay.
    /// </summary>
    /// <param name="message"></param>
    public void UpdateFeedbackText(string message)
    {
        feedbackText.text = message;
        clearFeedbackCoroutine = StartCoroutine(ClearFeedbackTextAfterDelay(3f));
    }

    /// <summary>
    /// Clears the feedback text after a specified delay.
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    public IEnumerator ClearFeedbackTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
        clearFeedbackCoroutine = null;
    }


    /// <summary>
    /// Updates the score text for a specific score type in the UI.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="score"></param>
    /// <param name="scoreType"></param>
    public void UpdateScoreText(string label, int score, string scoreType)
    {
        // Validating Checks
        if (GameObject.Find(label) == null)
        {
            Debug.LogError("UI element with label '" + label + "' not found. Please ensure it is assigned in the inspector.");
            return;
        } else if (GameObject.Find(label).GetComponent<TextMeshProUGUI>() == null)
        {
            Debug.LogError("UI element with label '" + label + "' does not have a TextMeshProUGUI component. Please ensure it is assigned in the inspector.");
            return;
        } else if (scoreType == null || scoreType == "")
        {
            Debug.LogError("Score type is null or empty. Please provide a valid score type.");
            return;
        }

        // Update the score text with the provided score and score type
        GameObject.Find(label).GetComponent<TextMeshProUGUI>().text = scoreType + ": " + score.ToString();
    }

    public void ActivateControllerCanvas()
    {
        controllerCanvas.gameObject.SetActive(true);
    }

    public void DeactivateControllerCanvas()
    {
        controllerCanvas.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (canvasesToRotate == null || canvasesToRotate.Length == 0)
        {
            return;
        }
        foreach (Canvas canvas in canvasesToRotate)
        {
            canvas.worldCamera = Camera.main;
            // Only rotate canvas on the Y-axis to face the camera, keeping the X and Z rotation fixed
            Vector3 rotation = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
            canvas.transform.rotation = Quaternion.Euler(rotation);
        }
    }
}
