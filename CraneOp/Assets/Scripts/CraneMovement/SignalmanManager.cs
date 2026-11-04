/*
    This script is responsible for managing the score and signalman signals in the game.
    It keeps track of the current signal, incorrect signals, and updates the UI accordingly.
*/
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SignalmanManager : MonoBehaviour
{
    public static SignalmanManager instance;

    [Header("Signalman Settings")]
    public TextMeshProUGUI signalmanSignal;
    public string currentSignal = "";
    public int incorrectSignals = 0;
    public int currentSignalIndex;
    public GameObject[] targets;
    
    [Header("Script References")]
    CraneSequence craneSequence;

    [Header("Load Settings")]
    public bool isLoadAttached = false;

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
        Events.Instance.onLoadLoaded += () => {isLoadAttached = true; ShowTarget(true);};
        Events.Instance.onLoadUnloaded += () => {isLoadAttached = false; ShowTarget(false);};
        
        craneSequence = FindObjectOfType<CraneSequence>();
        if (craneSequence == null)
        {
            Debug.LogError("CraneSequence script not found in the scene. Please ensure it is attached to a GameObject.");
        }

        // Initialize the signalman signal to the first target area
        currentSignalIndex = 0;

        // Ensure that there are target areas assigned in the inspector
        if (targets.Length == 0)
        {
            Debug.LogError("No target areas assigned to the SignalmanManager. Please assign target areas in the inspector.");
            return;
        }
        foreach (GameObject target in targets)
        {
            target.GetComponent<Collider>().enabled = false; // Disable the colliders for all target areas initially
            target.SetActive(false);
        }

        // Initialize the incorrect signals count in the UI
        UI.instance.UpdateScoreText("IncorrectSignalsText", incorrectSignals, "Incorrect Signals");
    }

    void LateUpdate()
    {
        if (!isLoadAttached)
        {
            currentSignal = "";
            signalmanSignal.text = "Waiting for load to be attached";
        }
        else
        {
            FindSignalDirection();
        }

        UI.instance.UpdateScoreText("IncorrectSignalsText", incorrectSignals, "Incorrect Signals");
    }
    
    /// <summary>
    /// Once the crane enters the target area, move on to the next area
    /// </summary>
    public void NextArea()
    {
        craneSequence.ActivateButtons(false);
        currentSignalIndex++;
        if (currentSignalIndex >= targets.Length)
        {
            UI.instance.UpdateFeedbackText("All target areas completed!");
            StartCoroutine(DelayToNextModule(3f));

            // Unsubscribe from events as the sequence is complete
            Events.Instance.onLoadLoaded -= () => {isLoadAttached = true; ShowTarget(true);};
            Events.Instance.onLoadUnloaded -= () => {isLoadAttached = false; ShowTarget(false);};

            targets[currentSignalIndex - 1].SetActive(false); // Hide the last target area

            return;
        }
        foreach (GameObject target in targets)
        {
            target.SetActive(false);
        }
        targets[currentSignalIndex].GetComponent<Collider>().enabled = true; // Enable the collider for the current target area
        targets[currentSignalIndex].SetActive(true);
    }

    /// <summary>
    /// Updates the Signalman signal based on current position of the crane with respect to the target area.
    /// </summary>
    public void FindSignalDirection()
    {
        if (currentSignalIndex >= targets.Length)
            return;

        Vector3 loadPosition = GameObject.Find("loadPos").transform.position;
        Vector3 targetPosition = targets[currentSignalIndex].transform.position;

        float xDifference = targetPosition.x - loadPosition.x;
        float zDifference = targetPosition.z - loadPosition.z;
        float yDifference = targetPosition.y - loadPosition.y;

        float horizontalTolerance = 0.5f;
        float verticalTolerance = 0.5f;

        bool xAligned = Mathf.Abs(xDifference) <= horizontalTolerance;
        bool zAligned = Mathf.Abs(zDifference) <= horizontalTolerance;

        // Final target: only hoist down.
        if (currentSignalIndex == targets.Length - 1)
        {
            currentSignal = "Down";
        }
        // Normal target movement.
        else if (!xAligned || !zAligned)
        {
            if (xAligned)
                currentSignal = zDifference > 0 ? "Front" : "Back";
            else if (zAligned)
                currentSignal = xDifference > 0 ? "Right" : "Left";
            else if (Mathf.Abs(xDifference) > Mathf.Abs(zDifference))
                currentSignal = xDifference > 0 ? "Right" : "Left";
            else
                currentSignal = zDifference > 0 ? "Front" : "Back";
        }
        else if (Mathf.Abs(yDifference) > verticalTolerance)
        {
            currentSignal = yDifference > 0 ? "Up" : "Down";
        }

        signalmanSignal.text = currentSignal;
    }

    void ShowTarget(bool show)
    {
        if (currentSignalIndex < targets.Length)
        {
            targets[currentSignalIndex].GetComponent<Collider>().enabled = show;
            targets[currentSignalIndex].SetActive(show);
        }
    }

    /// <summary>
    /// Delays the transition to the next module after a specified delay to ensure that the final feedback text is visible to the user.
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator DelayToNextModule(float delay)
    {
        yield return new WaitForSeconds(delay);
        Events.Instance.SequenceComplete();
    }
}
