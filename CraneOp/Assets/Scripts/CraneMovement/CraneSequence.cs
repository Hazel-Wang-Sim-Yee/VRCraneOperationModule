/*
    This script is responsible for managing the crane sequence in the game.
    It handles the activation and deactivation of buttons based on the crane's state.
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraneSequence : MonoBehaviour
{
    [Header("Controller Buttons (automatically assigned if not set)")]
    [SerializeField] List<Button> buttons = new List<Button>(); // Array of buttons to be activated/deactivated
    string currentSignal; // Current signal from the signalman
    public bool changedTarget = false; // Flag to indicate if the target has changed
    public bool isCraneMoving = false; // Flag to indicate if the crane is moving
    
    void Start()
    {
        if (buttons == null || buttons.Count == 0)
        {
            buttons.Add(GameObject.Find("Left").GetComponent<Button>());
            buttons.Add(GameObject.Find("Right").GetComponent<Button>());
            buttons.Add(GameObject.Find("Front").GetComponent<Button>());
            buttons.Add(GameObject.Find("Back").GetComponent<Button>());
            buttons.Add(GameObject.Find("Up").GetComponent<Button>());
            buttons.Add(GameObject.Find("Down").GetComponent<Button>());
        }
        currentSignal = SignalmanManager.instance.currentSignal;
        ActivateButtons(false); // Disable all buttons at the start of the sequence
        UI.instance.controllerCanvas.gameObject.SetActive(false);
    }
    void Update()
    {
        currentSignal = SignalmanManager.instance.currentSignal;
        if (SignalmanManager.instance.currentSignalIndex == 0 || SignalmanManager.instance.currentSignalIndex == SignalmanManager.instance.targets.Length - 1)
        {
            ActivateButtons(true); // Enable hoisting buttons if the crane is at the first or last target area
        } else
        {
            ActivateButtons(false); // Disable hoisting buttons if the crane is not at the first or last target area
        }
    }

    /// <summary>
    /// Activates or deactivates the buttons based on the crane's state.
    /// If the crane is hoisting, it disables movement buttons and enables hoist buttons.
    /// If the crane is not hoisting, it enables movement buttons and disables hoist buttons.
    /// </summary>
    /// <param name="isHoisting"></param>
    public void ActivateButtons(bool isHoisting)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].name == "Up" || buttons[i].name == "Down")
            {
                buttons[i].interactable = isHoisting;
            }
            else
            {
                buttons[i].interactable = !isHoisting;
            }
        }
    }

    /// <summary>
    /// Sets the state of the crane's movement.
    /// </summary>
    public void SetCraneMovingState()
    {
        isCraneMoving = !isCraneMoving;
    }
}