/*
    This script is responsible for moving the crane in the game.
    It allows the crane to move forward and backward based on user input.
*/
using UnityEngine;

public class CraneMove : MonoBehaviour
{
    public enum Direction
    {
        Back,
        Front,
    }
    private Vector3 targetPosition;
    public bool isMoving = false;

    public float moveSpeed = 1f;

    void Start()
    {
        Events.Instance.onMoveBack += MoveBack;
        Events.Instance.onMoveFront += MoveFront;
        Events.Instance.onStopMovement += StopMovement;
    }
    void Update()
    {
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
            isMoving = false;
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
            case Direction.Back:
                destination.z = GameObject.Find("BackBoundary").transform.position.z;
                break;

            case Direction.Front:
                destination.z = GameObject.Find("FrontBoundary").transform.position.z;
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
    public void MoveBack()
    {
        MoveToTarget(Direction.Back);
    }
    public void MoveFront()
    {
        MoveToTarget(Direction.Front);
    }
    public void StopMovement()
    {
        isMoving = false;
    }
}