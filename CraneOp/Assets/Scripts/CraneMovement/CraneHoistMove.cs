/*
    This script is responsible for moving the crane hoist with its attached hook and load in the game.
    It allows the crane hoist to move up and down based on user input, while ensuring it stays aligned with the trolley's x and z position.
*/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CraneHoistMove : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down
    }
    bool isMoving = false;
    float moveSpeed = 1f;
    Vector3 targetPosition;
    [SerializeField] Transform trolley;
    [SerializeField] Collider[] ropeColliders;
    public bool isHoistUp = false;
    [SerializeField] Rigidbody leftAttachRb;
    [SerializeField] Rigidbody rightAttachRb;
    Vector3 leftAttachOffset;
    Vector3 rightAttachOffset;

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
        loadPos.y = GameObject.Find("load").transform.position.y;
        loadPosition.position = loadPos;
        leftAttachOffset = leftAttachRb.transform.position - trolley.position;
        rightAttachOffset = rightAttachRb.transform.position - trolley.position;
    }
    void Update()
    {
        // Ensure the crane hoist stays at the same x and z position as the trolley
        Vector3 position = transform.position;
        Vector3 loadPos = loadPosition.position;
        position.z = loadPos.z = trolley.position.z;
        position.x = loadPos.x = trolley.position.x;
        transform.position = position;
        loadPosition.position = loadPos;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 nextPosition = transform.position;

        if (isMoving)
        {
            nextPosition = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.fixedDeltaTime
            );

            transform.position = nextPosition;
        }

        // Keep both attachment points aligned with the hoist,
        // including when the trolley moves horizontally.
        leftAttachRb.MovePosition(nextPosition + leftAttachOffset);
        rightAttachRb.MovePosition(nextPosition + rightAttachOffset);

        if (isMoving &&
            Vector3.Distance(nextPosition, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;

            UI.instance.UpdateFeedbackText(
                    isHoistUp
                        ? "Hoist is at its minimum length."
                        : "Hoist is at its maximum length."
                );

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
