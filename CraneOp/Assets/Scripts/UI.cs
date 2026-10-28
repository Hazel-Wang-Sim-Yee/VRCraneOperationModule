/*
    This script is used to manage UI elements in the game, such as feedback text and score display.
    It provides methods to update the feedback text with a message and clear it after a delay, as well as update the score text for different score types.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] TextMeshProUGUI feedbackText;
    Coroutine clearFeedbackCoroutine;

    [SerializeField] Image controllerCanvas;

    [Header("Singleton")]
    public static UI instance;

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
        if (controllerCanvas != null)
        {
            controllerCanvas.gameObject.SetActive(false);
        }
        Events.Instance.onLoadLoaded += ActivateControllerCanvas;
        Events.Instance.onLoadUnloaded += DeactivateControllerCanvas;
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
    IEnumerator ClearFeedbackTextAfterDelay(float delay)
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
}
