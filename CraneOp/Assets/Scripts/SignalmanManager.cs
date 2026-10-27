/*
    This script is responsible for managing the score and signalman signals in the game.
    It keeps track of the current signal, incorrect signals, and updates the UI accordingly.
*/
using UnityEngine;
using TMPro;

public class SignalmanManager : MonoBehaviour
{
    public static SignalmanManager instance;

    [Header("Signalman Settings")]
    public TextMeshProUGUI signalmanSignal;
    public string currentSignal = "";
    public int incorrectSignals = 0;

    enum SignalmanSignal
    {
        Left,
        Right,
        Back,
        Front,
        HoistUp,
        HoistDown,
    }
    public int currentSignalIndex;
    public GameObject[] targets;

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
        // Initialize the signalman signal to the first target area
        currentSignalIndex = 0;
        foreach (GameObject target in targets)
        {
            target.SetActive(false);
        }
        targets[currentSignalIndex].SetActive(true);
    }

    void Update()
    {
        // Update the signalman signal text in the UI
        FindSignalDirection();
        UI.instance.UpdateScoreText("IncorrectSignalsText", incorrectSignals, "Incorrect Signals");
    }

    /// <summary>
    /// Once the crane enters the target area, move on to the next area
    /// </summary>
    public void NextArea()
    {
        currentSignalIndex++;
        if (currentSignalIndex >= targets.Length)
        {
            UI.instance.UpdateFeedbackText("All target areas completed!");
            return;
        }
        foreach (GameObject target in targets)
        {
            target.SetActive(false);
        }
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

        // Check whether the load is aligned horizontally.
        bool xAligned = Mathf.Abs(xDifference) <= horizontalTolerance;
        bool zAligned = Mathf.Abs(zDifference) <= horizontalTolerance;

        if (!xAligned || !zAligned)
        {
            // Move along whichever horizontal axis still needs more adjustment.
            if (xAligned)
            {
                currentSignal = zDifference > 0 ? "Front" : "Back";
            }
            else if (zAligned)
            {
                currentSignal = xDifference > 0 ? "Right" : "Left";
            }
            else if (Mathf.Abs(xDifference) > Mathf.Abs(zDifference))
            {
                currentSignal = xDifference > 0 ? "Right" : "Left";
            }
            else
            {
                currentSignal = zDifference > 0 ? "Front" : "Back";
            }
        }
        else if (Mathf.Abs(yDifference) > verticalTolerance)
        {
            currentSignal = yDifference > 0 ? "Up" : "Down";
        }
        else
        {
            currentSignal = "Complete";
        }

        signalmanSignal.text = currentSignal;
    }
}
