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
        if (other.CompareTag("Load") && CompareTag("CraneDestination"))
        {
            SignalmanManager.instance.NextArea();
        }
        else if (other.CompareTag("Crane"))
        {
            Events.Instance.StopMovement();
        }
        if (other.CompareTag("Rope") && gameObject.name == "RopeGone")
        {
            // When the rope is goes up past a certain point, the rope mesh renderer is disabled to make it invisible.
            MeshRenderer ropeRenderer = other.GetComponent<MeshRenderer>();
            MeshRenderer ropeChildRenderer = other.transform.GetChild(0).GetComponent<MeshRenderer>();
            ropeRenderer.enabled = false;
            ropeChildRenderer.enabled = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Rope") && gameObject.name == "RopeGone")
        {
            // When the rope is goes down past a certain point, the rope mesh renderer is enabled to make it visible again.
            MeshRenderer ropeRenderer = other.GetComponent<MeshRenderer>();
            MeshRenderer ropeChildRenderer = other.transform.GetChild(0).GetComponent<MeshRenderer>();
            ropeRenderer.enabled = true;
            ropeChildRenderer.enabled = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Rope") && gameObject.name == "HoistUp")
        {
            // Ensure that the isTrigger property of the rope collider is set to true when the hoist is up to prevent physics problems.
            other.isTrigger = true;
            CraneHoistMove craneHoistMove = GameObject.Find("hoist").GetComponent<CraneHoistMove>();
            craneHoistMove.isHoistUp = true;
        }
    }
}