/*
    This script is responsible for detecting collisions and triggers in the game.
*/
using UnityEngine;
using System.Collections;

public class ColliderDetect : MonoBehaviour
{
    [Header("Collision Settings")]
    int collisions = 0;
    bool ableToCollide = true;
    Coroutine collisionCoroutine; // Store the coroutine to prevent multiple coroutines from running simultaneously
    float collisionCooldown = 1f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Load"))
        {
            if (ableToCollide)
            {
                collisionCoroutine = StartCoroutine(ResetCollision());
            }
        }
    }

    /// <summary>
    /// Coroutine to prevent multiple collisions from being counted within a short time frame.
    /// It increments the collision count, disables further collisions for a set cooldown period, and then re-enables collision detection.
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetCollision()
    {
        collisions++;
        ableToCollide = false;
        yield return new WaitForSeconds(collisionCooldown);
        ableToCollide = true;
        collisionCoroutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crane") && CompareTag("CraneDestination"))
        {
            SignalmanManager.instance.NextArea();
        }
        else if (other.CompareTag("Crane"))
        {
            CraneMove craneMove = FindObjectOfType<CraneMove>();
            if (craneMove.isMoving)
                craneMove.StopMovement();
        }
    }
}