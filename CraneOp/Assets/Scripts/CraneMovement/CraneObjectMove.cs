/*
    This script is responsible for moving the crane objects (trolley etc.) in the game.
    It allows the crane objects to move East and West based on user input, while ensuring they stay aligned with the crane's z position.
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
        East,
        West,
    }
    private Vector3 targetPosition;
    public bool isMoving = false;

    public float moveSpeed = 1f;
    Vector3 lastPos;
    public Vector3 lastCraneVelocity;
    public Rigidbody loadRB;
    [SerializeField] float swingStrength = 0.2f;
    [SerializeField] LoadHookSettings loadHookSettings;

    Transform crane;

    void Start()
    {
        lastPos = transform.position;
        crane = GameObject.Find("crane").transform;
        Events.Instance.onMoveEast += MoveEast;
        Events.Instance.onMoveWest += MoveWest;
        Events.Instance.onStopMovement += StopMovement;
    }

    void FixedUpdate()
    {
        if (loadRB == null) loadRB = loadHookSettings.loadRb;
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
            UI.Instance.UpdateFeedbackText("Crane can't move further.");
            StopMovement();
        }
        lastCraneVelocity = (transform.position - lastPos)/Time.fixedDeltaTime;
        lastPos = transform.position;
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
            case Direction.East:
                destination.x = GameObject.Find("LeftBoundary").transform.position.x;
                break;

            case Direction.West:
                destination.x = GameObject.Find("RightBoundary").transform.position.x;
                break;
        }

        // Sets the target position to the calculated destination
        targetPosition = destination;
        isMoving = true;
    }

    // Methods to move the crane in specific directions
    public void MoveEast()
    {
        MoveToTarget(Direction.East);
    }
    public void MoveWest()
    {
        MoveToTarget(Direction.West);
    }
    public void StopMovement()
    {
        isMoving = false;
        if (loadRB == null) return;
        Vector3 v = lastCraneVelocity;
        v.y = 0f;
        loadRB.AddForce(v*swingStrength, ForceMode.VelocityChange);
    }
}