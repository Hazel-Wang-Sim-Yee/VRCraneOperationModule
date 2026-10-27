using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] TextMeshProUGUI feedbackText;
    Coroutine clearFeedbackCoroutine;

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
    }

    public void UpdateFeedbackText(string message)
    {
        feedbackText.text = message;
        clearFeedbackCoroutine = StartCoroutine(ClearFeedbackTextAfterDelay(3f));
    }

    IEnumerator ClearFeedbackTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
        clearFeedbackCoroutine = null;
    }

    public void UpdateScoreText(string label, int score, string scoreType)
    {
        GameObject.Find(label).GetComponent<TextMeshProUGUI>().text = scoreType + ": " + score.ToString();
    }
}
