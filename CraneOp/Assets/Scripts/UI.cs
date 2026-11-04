/*
    This script is used to manage UI elements in the game, such as feedback text and score display.
    It provides methods to update the feedback text with a message and clear it after a delay, as well as update the score text for different score types.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] TextMeshProUGUI feedbackText;
    Coroutine clearFeedbackCoroutine;

    [Header("Controller Canvas")]
    [Tooltip("For crane modules, this is the canvas that contains the controller buttons.")]
    public Image controllerCanvas;

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
        feedbackText.text = "";
        
        // Subscribe based on current scene to avoid unnecessary subscriptions in non-relevant scenes
        if (SceneManager.GetActiveScene().name.Contains("Crane"))
        {
            // Ensure that the controller canvas is assigned in the inspector for crane modules
            if (controllerCanvas == null)
            {
                Debug.LogWarning("Ensure that the Controller Canvas is assigned if in crane modules.");
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
