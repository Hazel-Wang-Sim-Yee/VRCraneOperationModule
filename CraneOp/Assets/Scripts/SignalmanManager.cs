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
    }

    /// <summary>
    /// Once the crane enters the target area, move on to the next area
    /// </summary>
    public void NextArea()
    {
        currentSignalIndex++;
        if (currentSignalIndex >= targets.Length)
        {
            UI ui = FindObjectOfType<UI>();
            if (ui != null)
            {
                ui.UpdateFeedbackText("All target areas completed!");
            }
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

        // Get the current position of the crane and the target area
        Vector3 loadPosition = GameObject.Find("load").transform.position;
        Vector3 targetPosition = targets[currentSignalIndex].transform.position;

        // Calculate the difference in position between the crane and the target area
        float xDifference = targetPosition.x - loadPosition.x;
        float zDifference = targetPosition.z - loadPosition.z;
        float yDifference = targetPosition.y - loadPosition.y;

        if (Mathf.Abs(xDifference) > Mathf.Abs(zDifference))
        {
            currentSignal = xDifference > 0 ? "Right" : "Left";
        }
        else if (Mathf.Abs(yDifference) > Mathf.Abs(zDifference))
        {
            currentSignal = yDifference > 0 ? "Up" : "Down";
        }
        else
        {
            currentSignal = zDifference > 0 ? "Front" : "Back";
        }

        signalmanSignal.text = currentSignal;
    }
}
