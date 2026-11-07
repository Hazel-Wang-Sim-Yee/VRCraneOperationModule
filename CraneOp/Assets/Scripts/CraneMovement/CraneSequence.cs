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

public class CraneSequence : MonoBehaviour
{
    [Header("Controller Buttons (automatically assigned if not set)")]
    [SerializeField] List<Button> buttons = new List<Button>(); // Array of buttons to be activated/deactivated

    [Header("Target Tracking")]
    public bool isCraneMoving = false; // Flag to indicate if the crane is moving

    [Header("Operation State")]
    public bool hasOperationStarted = false; // Flag to indicate if the crane operation has started
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

        ActivateButtons(false); // Disable all buttons at the start of the sequence
        UI.instance.controllerCanvas.gameObject.SetActive(false);

        // Subscribe to the load loaded and sequence complete events to manage the load's interactable state
        Events.Instance.onLoadLoaded += () => {loadInteractable = loadHookSettings.load.GetComponent<XRGrabInteractable>(); SignalmanManager.instance.currentSignalIndex = 0;};
        Events.Instance.onSequenceComplete += CompleteOperation;
    }

    void Update()
    {
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
        // If crane already moved once and the operation has not started, set the operation as started
        if (!isCraneMoving && !hasOperationStarted)
        {
            hasOperationStarted = true;
            // Lock the load to not allow the player to interact with it throughout the operation
            loadInteractable.interactionLayers = InteractionLayerMask.GetMask("LockedLoad");
        }
        isCraneMoving = !isCraneMoving;
    }

    /// <summary>
    /// Completes the crane operation by unlocking the load for interaction and displaying the final score canvas with relevant statistics.
    /// </summary>
    void CompleteOperation()
    {
        loadInteractable.interactionLayers = InteractionLayerMask.GetMask("Load");
        UI.instance.finalScoreCanvas.gameObject.SetActive(true);
        UI.instance.UpdateScoreText("CollisionStat", FindObjectOfType<CollisionManager>().totalCollisions, "Total Collisions");
        UI.instance.UpdateScoreText("WrongSignalStat", SignalmanManager.instance.incorrectSignals, "Wrong Signals");
    }
}