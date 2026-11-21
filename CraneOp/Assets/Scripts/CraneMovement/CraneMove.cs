/*
    This script is responsible for moving the crane in the game.
    It allows the crane to move forward and backward based on user input.
*/
using UnityEngine;

// Runs before CraneObjectMove and CraneHoistMove each physics step, so everything below it follows without lag.
[DefaultExecutionOrder(-20)]
public class CraneMove : MonoBehaviour
{
    public enum Direction
    {
        South,
        North,
    }
    private Vector3 targetPosition;
    public bool isMoving = false;

    public float moveSpeed = 1f;

    void Start()
    {
        Events.Instance.onMoveSouth += MoveSouth;
        Events.Instance.onMoveFront += MoveNorth;
        Events.Instance.onStopMovement += StopMovement;
    }

    void FixedUpdate()
    {
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
            isMoving = false;
            UI.Instance.UpdateFeedbackText("Crane can't move further.");
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
            case Direction.South:
                destination.z = GameObject.Find("BackBoundary").transform.position.z;
                break;

            case Direction.North:
                destination.z = GameObject.Find("FrontBoundary").transform.position.z;
                break;
        }

        // Sets the target position to the calculated destination
        targetPosition = destination;
        isMoving = true;
    }

    // Methods to move the crane in specific directions
    public void MoveSouth()
    {
        MoveToTarget(Direction.South);
    }
    public void MoveNorth()
    {
        MoveToTarget(Direction.North);
    }
    public void StopMovement()
    {
        isMoving = false;
    }
}