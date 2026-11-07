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
    string pendingSignal;
    float pendingSignalTimer;
    float signalStayTime = 0.2f; // Time in seconds to wait before updating the signalman signal
    
    [Header("Script References")]
    CraneSequence craneSequence;

    [Header("Load Settings")]
    public bool isLoadAttached = false;
    [SerializeField] Transform loadPos; // Position of the load to determine its relation to the target area

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

    void Update()
    {
        if (!isLoadAttached)
        {
            signalmanSignal.text = "Waiting for load to be attached";
            return;
        }

        FindSignalDirection();

        UI.instance.UpdateScoreText("IncorrectSignalsText", incorrectSignals, "Incorrect Signals");
    }
    
    /// <summary>
    /// Once the crane enters the target area, move on to the next area
    /// </summary>
    public void NextArea()
    {
        if (craneSequence.hasOperationStarted == false)
        {
            return; // Ignore if the operation hasn't started
        }
        craneSequence.ActivateButtons(false);
        currentSignalIndex++;
        if (currentSignalIndex >= targets.Length)
        {
            UI.instance.UpdateFeedbackText("All target areas completed!");
            Events.Instance.SequenceComplete();

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
        if (currentSignalIndex >= targets.Length || loadPos == null) return;

        // Calculate the difference between the load's position and the current target area's position
        Vector3 diff = targets[currentSignalIndex].transform.position - loadPos.position;
        string wanted = ComputeSignal(diff);

        if (wanted == currentSignal)
        {
            // Check if the wanted signal is the same as the current signal. If so, clear the pending signal.
            pendingSignal = null;
        }
        else if (wanted != pendingSignal)
        {
            // If the wanted signal is different from the current signal and also different from the pending signal, set it as the new pending signal and start the timer.
            pendingSignal = wanted;
            pendingSignalTimer = Time.time; // Start the timer for the pending signal
        }
        else if (Time.time - pendingSignalTimer >= signalStayTime)
        {
            // If the pending signal has been held for the required time, update the current signal to the pending signal.
            currentSignal = wanted;
            pendingSignal = null;
        }

        signalmanSignal.text = currentSignal;
    }

    string ComputeSignal(Vector3 diff)
    {
        const float tolerance = 0.5f;

        // If the crane is at the first or last target area, return "Up" or "Down" respectively
        if (currentSignalIndex == 0) return "Up";
        if (currentSignalIndex == targets.Length - 1) return "Down";

        bool xAligned = Mathf.Abs(diff.x) <= tolerance;
        bool zAligned = Mathf.Abs(diff.z) <= tolerance;

        // Check if the load is aligned with either the x or z axis of the target area, and return the appropriate signal
        if (!xAligned || !zAligned)
        {
            // Determine which axis the load is more misaligned
            bool useX;
            if (xAligned) useX = false;
            else if (zAligned) useX = true;
            else
            {
                Debug.LogWarning("Unable to determine which axis to use for signal calculation. Defaulting to x-axis.");
                useX = true;
            }
            return useX ? (diff.x > 0 ? "Right" : "Left")
                        : (diff.z > 0 ? "Front" : "Back");
        }

        if (Mathf.Abs(diff.y) > tolerance) return diff.y > 0 ? "Up" : "Down";
        return currentSignal; // If the load is aligned with the target area, return the current signal
    }

    /// <summary>
    /// Shows or hides the current target area based on whether the load is attached and the current signal index.
    /// </summary>
    /// <param name="show"></param>
    void ShowTarget(bool show)
    {
        // Hide all target areas initially
        foreach (GameObject target in targets)
        {
            target.SetActive(false);
            target.GetComponent<Collider>().enabled = false;
        }

        // Show the current target area if the load is attached and the current signal index is valid
        if (currentSignalIndex < targets.Length)
        {
            targets[currentSignalIndex].GetComponent<Collider>().enabled = show;
            targets[currentSignalIndex].SetActive(show);
        }
    }
}
