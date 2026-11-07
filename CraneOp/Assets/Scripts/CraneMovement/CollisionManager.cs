/*
    This script is responsible for managing the collision detection of the crane operation module.
    It calculates the total number of collisions detected by all objects with the "CollisionObject" tag and updates the UI accordingly.
*/
using UnityEngine;
using System.Collections;

public class CollisionManager : MonoBehaviour
{
    [Header("Collision Settings")]
    public int totalCollisions;
    public GameObject[] collisionObjects;
    [SerializeField] int collisionLimit = 5; // Set a limit for the number of collisions

    [Header("Script References")]
    [SerializeField] SceneHandler sceneHandler;

    Coroutine restartSceneCoroutine; // Store the coroutine to prevent multiple coroutines from running simultaneously

    void Start()
    {
        StopAllCoroutines(); // Ensure no coroutines are running at the start
        if (sceneHandler == null)
        {
            sceneHandler = FindObjectOfType<SceneHandler>();
            if (sceneHandler == null)
            {
                Debug.LogError("SceneHandler script not found in the scene. Please ensure it is attached to a GameObject.");
            }
        }

        // Find all objects with the "CollisionObject" tag and store them in the collisionObjects array
        collisionObjects = GameObject.FindGameObjectsWithTag("CollisionObject");
        totalCollisions = 0;

        EnableCollisionDetection(false); // Disable collision detection at the start of the game

        Events.Instance.onLoadLoaded += () => {EnableCollisionDetection(true);};
        Events.Instance.onLoadUnloaded += () => {EnableCollisionDetection(false);};
    }

    void FixedUpdate()
    {
        CalculateTotalCollisions();
    }
    
    /// <summary>
    /// Calculates the total number of collisions detected by all objects with the "CollisionObject" tag and updates the UI accordingly.
    /// </summary>
    void CalculateTotalCollisions()
    {
        totalCollisions = 0;
        foreach (GameObject obj in collisionObjects)
        {
            ColliderDetect colliderDetect = obj.GetComponent<ColliderDetect>();
            if (colliderDetect != null)
            {
                totalCollisions += colliderDetect.collisions;
            }
        }
        if (totalCollisions >= collisionLimit)
        {
            // Handle the case where the collision limit is reached
            UI.instance.UpdateFeedbackText("You have made too many collisions! Please try again.");
            if (restartSceneCoroutine == null) // Check if the coroutine is already running
            {
                restartSceneCoroutine = StartCoroutine(RestartSceneAfterDelay(3f)); // Restart the scene after a 3-second delay
            }
        }
        UI.instance.UpdateScoreText("CollisionText", totalCollisions, "Collisions");
    }

    /// <summary>
    /// Enables or disables collision detection for all collision objects.
    /// Add/remove Rigidbody components to prevent or allow the obstacles from falling before the operation starts.
    /// </summary>
    /// <param name="enable"></param>
    void EnableCollisionDetection(bool enable)
    {
        foreach (GameObject obj in collisionObjects)
        {
            if (enable)
            {
                // Add a Rigidbody component and configure its properties
                obj.AddComponent<Rigidbody>();
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                rb.mass = 0.1f;
                rb.linearDamping = 0f;
                rb.angularDamping = 0.05f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.useGravity = true;
            } else
            {
                // Remove the Rigidbody component of it has one
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Destroy(rb);
                }
            }

            ColliderDetect colliderDetect = obj.GetComponent<ColliderDetect>();
            if (colliderDetect != null)
            {
                colliderDetect.enabled = enable;
            }
        }
    }
    
    IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        sceneHandler.ModuleSelection(sceneHandler.currentModuleIndex); // Restart the current module
        restartSceneCoroutine = null; // Reset the coroutine reference
    }
}