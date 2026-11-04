/*
    This script is responsible for moving the crane hoist with its attached hook and load in the game.
    It allows the crane hoist to move up and down based on user input, while ensuring it stays aligned with the trolley's x and z position.
*/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Runs after CraneMove and CraneObjectMove each physics step, so the trolley has already moved.
[DefaultExecutionOrder(0)]
public class CraneHoistMove : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down
    }
    public bool isMoving = false;
    float moveSpeed = 1f;
    Vector3 targetPosition;
    [SerializeField] Transform trolley;
    public bool isHoistUp = false;

    [Header("Attachment Points Settings")]
    /* Adding more attachment points
    [SerializeField] Rigidbody [attachName]Rb;
    Vector3 [attachName]Offset;
    */

    // These must be KINEMATIC Rigidbodies and should not be children of anything moved by transform.
    [SerializeField] Rigidbody leftAttachRb;
    [SerializeField] Rigidbody rightAttachRb;
    Vector3 leftAttachOffset;
    Vector3 rightAttachOffset;

    [Header("Load Position Settings")]
    [SerializeField] Transform loadPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Events.Instance.onHoistUp += HoistUp;
        Events.Instance.onHoistDown += HoistDown;
        Events.Instance.onStopMovement += StopMovement;
        Vector3 position = transform.position;
        position.x = trolley.position.x;
        position.z = trolley.position.z;
        transform.position = position;
        Vector3 loadPos = loadPosition.position;

        /* Adding more attachment points
        [attachName]Offset = [attachName]Rb.position - transform.position;
        */

        // Offsets are measured from the HOIST (not the trolley) so the attach points don't jump
        // if the hoist and trolley sit at different heights.
        leftAttachOffset = leftAttachRb.position - transform.position;
        rightAttachOffset = rightAttachRb.position - transform.position;
    }

    // Only the non-physics marker is updated here.
    void Update()
    {
        Vector3 loadPos = loadPosition.position;
        loadPos.x = trolley.position.x;
        loadPos.z = trolley.position.z;
        loadPosition.position = loadPos;
    }

    void FixedUpdate()
    {
        // Follow the trolley on x/z.
        Vector3 nextPosition = transform.position;
        nextPosition.x = trolley.position.x;
        nextPosition.z = trolley.position.z;

        // The hoist only moves in Y. (The old target also held an out-of-date x/z, which
        // dragged the hoist sideways and could stop it from ever "arriving".)
        bool arrived = false;
        if (isMoving)
        {
            nextPosition.y = Mathf.MoveTowards(
                nextPosition.y,
                targetPosition.y,
                moveSpeed * Time.fixedDeltaTime
            );
            arrived = Mathf.Abs(nextPosition.y - targetPosition.y) < 0.001f;
            if (arrived) nextPosition.y = targetPosition.y;
        }
        transform.position = nextPosition;

        // Keep both attachment points aligned with the hoist,
        // including when the trolley moves horizontally.

        /* Adding more attachment points
        [attachName]Rb.MovePosition(nextPosition + [attachName]Offset);
        */

        leftAttachRb.MovePosition(nextPosition + leftAttachOffset);
        rightAttachRb.MovePosition(nextPosition + rightAttachOffset);

        if (arrived)
        {
            // If crane reached last target area, do not display feedback text for hoist up or down.
            if (SignalmanManager.instance.currentSignalIndex < SignalmanManager.instance.targets.Length -1)
            {
                UI.instance.UpdateFeedbackText(isHoistUp ? "Hoist is at its minimum length." : "Hoist is at its maximum height.");
            }
            StopMovement();
        }
    }
    void MoveToTarget(Direction direction)
    {
        // Get the current position of the crane hoist
        Vector3 destination = transform.position;

        // Check the direction the crane hoist wants to move and set the destination accordingly
        switch (direction)
        {
            case Direction.Up:
                destination.y = GameObject.Find("HoistUp").transform.position.y;
                isHoistUp = true;
                break;

            case Direction.Down:
                destination.y = GameObject.Find("HoistDown").transform.position.y;
                isHoistUp = false;
                break;
        }

        if (SignalmanManager.instance.currentSignal != direction.ToString())
        {
            SignalmanManager.instance.incorrectSignals++;
        }
        // Set the target position to the calculated destination
        targetPosition = destination;
        isMoving = true;
    }
    public void HoistUp()
    {
        MoveToTarget(Direction.Up);
        // Prevent the incorrect signal count from counting twice when the button is pressed and released
        if (GameObject.Find("Up").GetComponent<ControllerState>().isButtonPressed == true)
        {
            if (SignalmanManager.instance.currentSignal != Direction.Up.ToString())
            {
                SignalmanManager.instance.incorrectSignals--;
            }
        }
    }
    public void HoistDown()
    {
        MoveToTarget(Direction.Down);
        // Prevent the incorrect signal count from counting twice when the button is pressed and released
        if (GameObject.Find("Down").GetComponent<ControllerState>().isButtonPressed == true)
        {
            if (SignalmanManager.instance.currentSignal != Direction.Down.ToString())
            {
                SignalmanManager.instance.incorrectSignals--;
            }
        }
    }
    void StopMovement()
    {
        isMoving = false;
    }

}