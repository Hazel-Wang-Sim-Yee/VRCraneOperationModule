/*
    This script is used to manage the state of the controller buttons in the UI.
    (NEED TO FIX) It changes the color of the button when it is pressed and disables other buttons to prevent multiple movements at once.
    It also listens for events that indicate movement in different directions and toggles the button state accordingly.
*/
using UnityEngine;
using UnityEngine.UI;

public class ControllerState : MonoBehaviour
{
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;
    public bool isButtonPressed = false;
    private string buttonName;
    [SerializeField] Button[] otherButtons;

    void Start()
    {
        buttonName = gameObject.name;
        if (buttonName == "Left")
        {
            Events.Instance.onMoveLeft += ToggleButtonState;
        }
        else if (buttonName == "Right")
        {
            Events.Instance.onMoveRight += ToggleButtonState;
        }
        else if (buttonName == "Front")
        {
            Events.Instance.onMoveFront += ToggleButtonState;
        }
        else if (buttonName == "Back")
        {
            Events.Instance.onMoveBack += ToggleButtonState;
        }
        else if (buttonName == "Up")
        {
            Events.Instance.onHoistUp += ToggleButtonState;
        }
        else if (buttonName == "Down")
        {
            Events.Instance.onHoistDown += ToggleButtonState;
        }
    }

    /// <summary>
    /// Toggles the state of the button when it is pressed.
    /// If the button is pressed, it disables other buttons to prevent multiple movements at once.
    /// If the button is released, it re-enables the other buttons. It also updates the button color based on its state.
    /// </summary>
    public void ToggleButtonState()
    {
        isButtonPressed = !isButtonPressed;
        if (isButtonPressed == false)
        {
            foreach (Button button in otherButtons)
            {
                button.interactable = true;
            }
            Events.Instance.StopMovement();
        } else if (isButtonPressed == true)
        {
            foreach (Button button in otherButtons)
            {
                button.interactable = false;
            }
        }
        UpdateButtonColor();
    }

    /// <summary>
    /// Updates the color of the button based on its state (pressed or not pressed).
    /// If the button is pressed, it sets the color to activeColor; otherwise, it sets it to inactiveColor.
    /// This is used to provide visual feedback to the user about the button's state.
    /// </summary>
    void UpdateButtonColor()
    {
        Button button = GetComponent<Button>();

        if (button == null)
            return;

        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = isButtonPressed ? activeColor : inactiveColor;
        button.colors = colorBlock;
    }
}
