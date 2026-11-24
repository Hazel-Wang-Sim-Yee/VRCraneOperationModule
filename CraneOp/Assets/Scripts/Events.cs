/*
    This script is used to manage events related to crane movement in the game.
    Other scripts can subscribe to these events to respond to user input or other triggers.
*/
using UnityEngine;
using System;

[DefaultExecutionOrder(-100)]
public class Events : MonoBehaviour
{
public static Events Instance { get; private set; }

    // Crane Movement Events
    public event Action onMoveEast;
    public event Action onMoveWest;
    public event Action onMoveFront;
    public event Action onMoveSouth;
    public event Action onStopMovement;
    public event Action onHoistUp;
    public event Action onHoistDown;

    // Load Events
    public event Action onLoadLoaded;
    public event Action onLoadUnloaded;

    // Sequence Events
    public event Action onSequenceComplete;
    public event Action onItemCorrect;
    public event Action onItemWrong;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Crane Movement Events
    public void MoveEast()
    {
        onMoveEast?.Invoke();
    }

    public void MoveWest()
    {
        onMoveWest?.Invoke();
    }

    public void MoveNorth()
    {
        onMoveFront?.Invoke();
    }

    public void MoveSouth()
    {
        onMoveSouth?.Invoke();
    }

    public void StopMovement()
    {
        onStopMovement?.Invoke();
    }

    public void HoistUp()
    {
        onHoistUp?.Invoke();
    }

    public void HoistDown()
    {
        onHoistDown?.Invoke();
    }

    // Load Events
    public void LoadLoaded()
    {
        onLoadLoaded?.Invoke();
    }

    public void LoadUnloaded()
    {
        onLoadUnloaded?.Invoke();
    }

    // Sequence Events
    public void SequenceComplete()
    {
        onSequenceComplete?.Invoke();
    }

    public void ItemCorrect()
    {
        onItemCorrect?.Invoke();
    }

    public void ItemWrong()
    {
        onItemWrong?.Invoke();
    }
}
