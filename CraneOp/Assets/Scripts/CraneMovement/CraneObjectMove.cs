/*
    This script is responsible for moving the crane objects (trolley etc.) in the game.
    It allows the crane objects to move left and right based on user input, while ensuring they stay aligned with the crane's z position.
    NOTE: put this on the trolley / non-physics crane parts only. Never on the hook or the load:
    those are dynamic Rigidbodies and must only be moved by their joints.
*/
using UnityEngine;

// Runs after CraneMove and before CraneHoistMove each physics step.
[DefaultExecutionOrder(-10)]
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

    Transform crane;

    void Start()
    {
        crane = GameObject.Find("crane").transform;
        Events.Instance.onMoveLeft += MoveLeft;
        Events.Instance.onMoveRight += MoveRight;
        Events.Instance.onStopMovement += StopMovement;
    }

    void FixedUpdate()
    {
        // Ensure the crane objects stay at the same z position as the crane
        Vector3 position = transform.position;
        position.z = crane.position.z;
        transform.position = position;
        if (!isMoving)
            return;
        // If the crane is moving, move it towards the target position
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        if (transform.position == targetPosition)
        {
            UI.instance.UpdateFeedbackText("Crane can't move further.");
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