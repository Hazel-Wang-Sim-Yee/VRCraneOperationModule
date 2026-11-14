/*
    This script is responsible for managing the crane sequence in the game.
    It keeps track of the current signal from the signalman, manages the state of the crane's movement, and enables or disables the appropriate buttons based on the crane's state.
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Rendering;

public class CraneSequence : MonoBehaviour
{
    [SerializeField] Volume dangerZone;

    [Header("Controller Buttons (automatically assigned if not set)")]
    public List<Button> buttons = new List<Button>(); // Array of buttons to be activated/deactivated

    [Header("Target Tracking")]
    public bool isCraneMoving = false; // Flag to indicate if the crane is moving

    [Header("Operation State")]
    public bool startButtonPressed = false; // Flag to indicate if the start button has been pressed
    public bool isHookReady = false;
    public bool hasOperationStarted = false; // Flag to indicate if the crane operation has started
    public bool hasOperationCompleted = false; // Flag to indicate if the crane operation has completed
    XRGrabInteractable loadInteractable; // Reference to the XRGrabInteractable component of the load

    [Header("Script References")]
    [SerializeField] LoadHookSettings loadHookSettings; // Reference to the LoadHookSettings script to manage the load's state
    
    void Start()
    {
        if (loadHookSettings == null)
        {
            loadHookSettings = FindObjectOfType<LoadHookSettings>();
            if (loadHookSettings == null)
            {
                Debug.LogError("LoadHookSettings script not found in the scene. Please ensure it is attached to a GameObject.");
            }
        }
        
        // Auto-find might be broken at times so assign the buttons manually in the inspector to prevent issues.
        if (buttons == null || buttons.Count == 0)
        {
            buttons.Add(GameObject.Find("Left").GetComponent<Button>());
            buttons.Add(GameObject.Find("Right").GetComponent<Button>());
            buttons.Add(GameObject.Find("Front").GetComponent<Button>());
            buttons.Add(GameObject.Find("Back").GetComponent<Button>());
            buttons.Add(GameObject.Find("Up").GetComponent<Button>());
            buttons.Add(GameObject.Find("Down").GetComponent<Button>());
        }

        foreach (Button button in buttons)
        {
            button.interactable = false; // Disable all buttons at the start of the sequence
        }

        dangerZone.weight = 0; // Disable the danger zone at the start of the sequence
        // Subscribe to the load loaded and sequence complete events to manage the load's interactable state
        Events.Instance.onLoadLoaded += () => {loadInteractable = loadHookSettings.load.GetComponent<XRGrabInteractable>(); SignalmanManager.instance.currentSignalIndex = 0; dangerZone.weight = 1;};
        Events.Instance.onLoadUnloaded += () => {dangerZone.weight = 0;};
        Events.Instance.onSequenceComplete += CompleteOperation;
    }

    void Update()
    {
        // If the load is not attached or the start button has not been pressed, return early to prevent further processing
        if (!SignalmanManager.instance.isLoadAttached) return;
        if (!startButtonPressed) return;

        if (SignalmanManager.instance.currentSignalIndex == 0 || SignalmanManager.instance.currentSignalIndex == SignalmanManager.instance.targets.Length - 1)
        {
            ActivateHoistButtons(true); // Enable hoisting buttons if the crane is at the last target area
        } else
        {
            ActivateHoistButtons(false); // Disable hoisting buttons if the crane is not at the first or last target area
        }
    }

    /// <summary>
    /// Activates or deactivates the buttons based on the crane's state.
    /// If the crane is hoisting, it disables movement buttons and enables hoist buttons.
    /// If the crane is not hoisting, it enables movement buttons and disables hoist buttons.
    /// </summary>
    /// <param name="isHoisting"></param>
    public void ActivateHoistButtons(bool isHoisting)
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
    public void SetCraneMovingState(bool moving)
    {
        isCraneMoving = moving;
        if (!isHookReady) return;
        // If crane already moved once and the operation has not started, set the operation as started
        if (isCraneMoving && hasOperationStarted)
        {
            // Lock the load to not allow the player to interact with it throughout the operation
            loadInteractable.interactionLayers = InteractionLayerMask.GetMask("LockedLoad");
        } else
        {
            loadInteractable.interactionLayers = InteractionLayerMask.GetMask("Load");
        }
    }

    /// <summary>
    /// Completes the crane operation by unlocking the load for interaction and displaying the final score canvas with relevant statistics.
    /// </summary>
    void CompleteOperation()
    {
        hasOperationCompleted = true;
        loadInteractable.interactionLayers = InteractionLayerMask.GetMask("Load");
        UI.instance.finalScoreCanvas.gameObject.SetActive(true);
        UI.instance.UpdateScoreText("CollisionStat", FindObjectOfType<CollisionManager>().totalCollisions, "Total Collisions");
        UI.instance.UpdateScoreText("WrongSignalStat", SignalmanManager.instance.incorrectSignals, "Wrong Signals");
    }


    /// <summary>
    /// Starts the crane operation when the start button is pressed.
    /// If the start button is pressed, it enables the hoisting buttons if the hook is not ready.
    /// </summary>
    public void StartOperation(bool start)
    {
        startButtonPressed = start;
        if (startButtonPressed)
        {
            hasOperationStarted = true; // Set the operation state to started when the start button is pressed
            if (!isHookReady)
            {
                ActivateHoistButtons(true); // Enable hoisting buttons if the hook is not ready
            }
        } else
        {
            EnableAllButtons(false); // Disable all buttons if the start button is not pressed
            hasOperationStarted = false; // Reset the operation state if the start button is not pressed
        }

        UI.instance.UpdateFeedbackText(startButtonPressed ? "Operation Started!" : "Operation Stopped!");
    }
    
    /// <summary>
    /// Enables or disables all buttons in the crane sequence.
    /// </summary>
    /// <param name="enable"></param>
    public void EnableAllButtons(bool enable)
    {
        foreach (Button button in buttons)
        {
            button.interactable = enable;
        }
    }
}