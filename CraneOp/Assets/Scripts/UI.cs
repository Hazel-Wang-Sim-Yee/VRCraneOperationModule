using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI feedbackText;
    Coroutine clearFeedbackCoroutine;

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
}
