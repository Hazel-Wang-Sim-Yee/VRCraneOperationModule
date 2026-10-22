/*
    This script is responsible for moving the crane objects (hook, load, etc.) in the game.
    It allows the crane to move left, right, up, and down based on user input.
*/
using UnityEngine;

public class CraneObjectMove : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right,
        HoistUp,
        HoistDown
    }
    [SerializeField] GameObject rope;
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
            case Direction.Left:
                destination.x = GameObject.Find("LeftBoundary").transform.position.x;
                break;

            case Direction.Right:
                destination.x = GameObject.Find("RightBoundary").transform.position.x;
                break;
            case Direction.HoistUp:
                rope.transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
                break;
            case Direction.HoistDown:
                rope.transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
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
    }
    public void MoveRight()
    {
        MoveToTarget(Direction.Right);
    }
    public void StopMovement()
    {
        // Ensure both the crane and the crane objects stop moving
        isMoving = false;
        CraneMove craneMove = FindObjectOfType<CraneMove>();
        if (craneMove != null)
        {
            craneMove.StopMovement();
        }
    }
}
