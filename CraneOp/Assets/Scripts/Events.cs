/*
    This script is used to manage events related to crane movement in the game.
    Other scripts can subscribe to these events to respond to user input or other triggers.
*/
using UnityEngine;
using System;

public class Events : MonoBehaviour
{
public static Events Instance { get; private set; }

    public event Action onMoveLeft;
    public event Action onMoveRight;
    public event Action onMoveFront;
    public event Action onMoveBack;
    public event Action onStopMovement;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void MoveLeft()
    {
        onMoveLeft?.Invoke();
    }

    public void MoveRight()
    {
        onMoveRight?.Invoke();
    }

    public void MoveFront()
    {
        onMoveFront?.Invoke();
    }

    public void MoveBack()
    {
        onMoveBack?.Invoke();
    }

    public void StopMovement()
    {
        onStopMovement?.Invoke();
    }
}
