/*
    This script is responsible for moving the crane hoist with its attached hook and load in the game.
    It allows the crane hoist to move up and down based on user input, while ensuring it stays aligned with the trolley's x and z position.
    Note: To add more rope attached to the hook and trolley, re-adjust the leftAttach and rightAttach positions and add additional attach points (how many ropes you want to add)
    and duplicate the ropeSegments' parent and reposition them accordingly. Then the 1st rope segment is attached to the attach point of the trolley, the last rope segment is attached to the character joint of the hook (add more character joints),
    and the rest of the rope segments are attached to each other in between. If you wish to reconfigure the hook attachment, you may try to use the method of attachment as the trolley attachments.
*/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CraneHoistMove : MonoBehaviour
{
    public enum Direction
    {
        HoistUp,
        HoistDown
    }
    bool isMoving = false;
    float moveSpeed = 1f;
    Vector3 targetPosition;
    [SerializeField] Transform trolley;
    [SerializeField] Collider[] ropeColliders;
    public bool isHoistUp = false;

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
    }
    void LateUpdate()
    {
        // Ensure the crane hoist stays at the same x and z position as the trolley
        Vector3 position = transform.position;
        position.z = trolley.position.z;
        position.x = trolley.position.x;
        transform.position = position;
    }
    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
            return;
        // If the crane hoist is moving, move it towards the target position
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
                if (isHoistUp)
                    ui.UpdateFeedbackText("Hoist is at its minimum length.");
                else
                    ui.UpdateFeedbackText("Hoist is at its maximum length.");
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
            case Direction.HoistUp:
                destination.y = GameObject.Find("HoistUp").transform.position.y;
                isHoistUp = true;
                break;

            case Direction.HoistDown:
                destination.y = GameObject.Find("HoistDown").transform.position.y;
                isHoistUp = false;
                break;
        }

        if (SignalmanManager.instance.currentSignal != Direction.HoistUp.ToString())
        {
            SignalmanManager.instance.incorrectSignals++;
        }
        // Set the target position to the calculated destination
        targetPosition = destination;
        foreach (Collider ropeCollider in ropeColliders)
        {
            ropeCollider.isTrigger = true;
        }
        isMoving = true;
    }
    public void HoistUp()
    {
        MoveToTarget(Direction.HoistUp);
        // Prevent the incorrect signal count from counting twice when the button is pressed and released
        if (GameObject.Find("Up").GetComponent<ControllerState>().isButtonPressed == true)
        {
            if (SignalmanManager.instance.currentSignal != Direction.HoistUp.ToString())
            {
                SignalmanManager.instance.incorrectSignals--;
            }
        }
    }
    public void HoistDown()
    {
        MoveToTarget(Direction.HoistDown);
        // Prevent the incorrect signal count from counting twice when the button is pressed and released
        if (GameObject.Find("Down").GetComponent<ControllerState>().isButtonPressed == true)
        {
            if (SignalmanManager.instance.currentSignal != Direction.HoistDown.ToString())
            {
                SignalmanManager.instance.incorrectSignals--;
            }
        }
    }
    void StopMovement()
    {
        isMoving = false;
        if (!isHoistUp)
        {
            foreach (Collider ropeCollider in ropeColliders)
            {
                ropeCollider.isTrigger = false;
            }
        }
    }
}
