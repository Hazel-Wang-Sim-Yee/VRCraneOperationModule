/*
    This script is used to manage the state of the controller buttons in the UI.
    It changes the color of the button when it is pressed and disables other buttons to prevent multiple movements at once.
    It also listens for events that indicate movement in different directions and toggles the button state accordingly.
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ControllerState : MonoBehaviour
{
    [Header("Colour")]
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;

    [Header("Button Settings")]
    public bool isButtonPressed = false;
    private string buttonName;
    [SerializeField] List<ControllerState> otherButtonStates;
    [SerializeField] AudioClip craneMoveAudio;
    [SerializeField] AudioSource audioSource;

    [Header("Script References")]
    CraneSequence craneSequence;
    CraneHoistMove craneHoistMove;

    void Start()
    {
        craneSequence = FindAnyObjectByType<CraneSequence>();
        craneHoistMove = FindAnyObjectByType<CraneHoistMove>();
        Events.Instance.onStopMovement += ResetState;
        buttonName = gameObject.name;
    }
    
    /// <summary>
    /// Toggles the state of the button when it is pressed.
    /// If the button is pressed, it disables other buttons to prevent multiple movements at once.
    /// If the button is released, it re-enables the other buttons. It also updates the button color based on its state.
    /// </summary>
    public void ToggleButtonState()
    {
        foreach (ControllerState state in otherButtonStates)
            if (state.isButtonPressed)
                return;
        
        isButtonPressed = !isButtonPressed;

        if (isButtonPressed)
        {
            switch (buttonName)
            {
                case "East":  Events.Instance.MoveEast();  break;   // use your real Events method names
                case "West": Events.Instance.MoveWest(); break;
                case "North": Events.Instance.MoveNorth(); break;
                case "South":  Events.Instance.MoveSouth();  break;
                case "Up":    Events.Instance.HoistUp();   break;
                case "Down":  Events.Instance.HoistDown(); break;
            }
            
            UpdateButtonColor();
            craneSequence.SetCraneMovingState(true);
            SignalmanManager.Instance.CheckSignal(buttonName);

            if (audioSource.clip != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
            audioSource.clip = craneMoveAudio;
            audioSource.Play();
        }
        else if (isButtonPressed == false)
        {
            Events.Instance.StopMovement();
            UpdateButtonColor();
            craneSequence.SetCraneMovingState(false);
            audioSource.Stop();
            audioSource.clip = null;
        }
        else
            Debug.LogError("isButtonPressed has an invalid state.");
    }

    /// <summary>
    /// Updates the color of the button based on its state (pressed or not pressed).
    /// If the button is pressed, it sets the color to activeColor; otherwise, it sets it to inactiveColor.
    /// This is used to provide visual feedback to the user about the button's state.
    /// </summary>
    void UpdateButtonColor()
    {
        Image image = GetComponent<Image>();

        if (image == null)
            return;

        image.color = isButtonPressed ? activeColor : inactiveColor;
    }

    void ResetState()
    {
        isButtonPressed = false;
        GetComponent<Image>().color = inactiveColor;
        audioSource.clip = null;
        audioSource.Stop();
    }
}
