/*
    This script is responsible for detecting collisions and triggers in the game.
*/
using UnityEngine;
using System.Collections;

public class ColliderDetect : MonoBehaviour
{
    [Header("Collision Settings")]
    public int collisions = 0;
    bool ableToCollide = true;
    Coroutine collisionCoroutine; // Store the coroutine to prevent multiple coroutines from running simultaneously
    float collisionCooldown = 1f;

    [Header("Script References")]
    [SerializeField] private CraneSequence craneSequence;

    [Header("Colour Settings")]
    [SerializeField] private Material insideColor;
    [SerializeField] private Material outsideColor;
    
    void Start()
    {
        if (craneSequence == null)
        {
            craneSequence = FindObjectOfType<CraneSequence>();
            if (craneSequence == null)
            {
                Debug.LogError("CraneSequence script not found in the scene. Please ensure it is attached to a GameObject.");
            }
        }
        if (insideColor == null)
        {
            Debug.LogWarning("Inside color not assigned. Please assign a material in the inspector if needed.");
        }
        if (outsideColor == null)
        {
            Debug.LogWarning("Outside color not assigned. Please assign a material in the inspector if needed.");
        }
    }

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
            // Indicate that the load is in position and change the color of the crane destination to green.
            gameObject.GetComponent<MeshRenderer>().material = insideColor;
            if (craneSequence.changedTarget == true)
            {
                craneSequence.changedTarget = false;
            }
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
        if (other.CompareTag("Load") && CompareTag("CraneDestination"))
        {
            gameObject.GetComponent<MeshRenderer>().material = outsideColor;
            // Reset the changedTarget flag in the CraneSequence script when the load exits the crane destination trigger.
            if (craneSequence.changedTarget == true)
            {
                craneSequence.changedTarget = false;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Rope") && gameObject.name == "HoistUp")
        {
            CraneHoistMove craneHoistMove = GameObject.Find("hoist").GetComponent<CraneHoistMove>();
            craneHoistMove.isHoistUp = true;
        }
        if (other.CompareTag("Load")&& CompareTag("CraneDestination"))
        {
            gameObject.GetComponent<MeshRenderer>().material = insideColor;
            // Check if the crane is moving or not and if its not moving, check if the target has already been changed. If not, change the target to the next area.
            if (craneSequence.isCraneMoving == false && craneSequence.changedTarget == false)
            {
                craneSequence.changedTarget = true;
                SignalmanManager.instance.NextArea();
            }
        }
    }
}