/*
    This script is responsible for moving the crane objects (hook, load, etc.) in the game.
    It allows the crane objects to move left and right based on user input, while ensuring they stay aligned with the crane's z position.
*/
using UnityEngine;

public class CraneObjectMove : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right,
    }
    private Vector3 targetPosition;
    public bool isMoving = false;

    public float moveSpeed = 1f;

    void Start()
    {
        Events.Instance.onMoveLeft += MoveLeft;
        Events.Instance.onMoveRight += MoveRight;
        Events.Instance.onStopMovement += StopMovement;
    }
    void Update()
    {
        // Ensure the crane objects stay at the same z position as the crane
        Vector3 position = transform.position;
        position.z = GameObject.Find("crane").transform.position.z;
        transform.position = position;
        if (!isMoving)
            return;
        // If the crane is moving, move it towards the target position
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (transform.position == targetPosition)
        {
            UI ui = FindObjectOfType<UI>();
            if (ui != null)
            {
                ui.UpdateFeedbackText("Crane can't move further.");
            }
            StopMovement();
        }
    }

    /// <summary>
    /// Moves the crane to the specified direction.
    /// </summary>
    /// <param name="direction"></param>
    public void MoveToTarget(Direction direction)
    {
        Vector3 destination = transform.position; // Gets the current position of the crane

        // Checks the direction the crane wants to move and sets the destination accordingly
        switch (direction)
        {
            // Find the respective boundary game object and set the destination to its position (x or z)
            case Direction.Left:
                destination.x = GameObject.Find("LeftBoundary").transform.position.x;
                break;

            case Direction.Right:
                destination.x = GameObject.Find("RightBoundary").transform.position.x;
                break;
        }
        if (SignalmanManager.instance.currentSignal != direction.ToString())
        {
            SignalmanManager.instance.incorrectSignals++;
        }
        // Sets the target position to the calculated destination
        targetPosition = destination;
        isMoving = true;
    }

    // Methods to move the crane in specific directions
    public void MoveLeft()
    {
        MoveToTarget(Direction.Left);
        // Prevent the incorrect signal count from counting twice when the button is pressed and released
        if (GameObject.Find("Left").GetComponent<ControllerState>().isButtonPressed == true)
        {
            if (SignalmanManager.instance.currentSignal != Direction.Left.ToString())
            {
                SignalmanManager.instance.incorrectSignals--;
            }
        }
    }
    public void MoveRight()
    {
        MoveToTarget(Direction.Right);
        // Prevent the incorrect signal count from counting twice when the button is pressed and released
        if (GameObject.Find("Right").GetComponent<ControllerState>().isButtonPressed == true)
        {
            if (SignalmanManager.instance.currentSignal != Direction.Right.ToString())
            {
                SignalmanManager.instance.incorrectSignals--;
            }
        }
    }
    public void StopMovement()
    {
        isMoving = false;
    }
}
