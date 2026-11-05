/*
    This script is responsible for managing the collision detection of the crane operation module.
    It calculates the total number of collisions detected by all objects with the "CollisionObject" tag and updates the UI accordingly.
*/
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public int totalCollisions;
    public GameObject[] collisionObjects;

    void Start()
    {
        // Find all objects with the "CollisionObject" tag and store them in the collisionObjects array
        collisionObjects = GameObject.FindGameObjectsWithTag("CollisionObject");
        totalCollisions = 0;
        foreach(GameObject obj in collisionObjects)
        {
            ColliderDetect colliderDetect = obj.GetComponent<ColliderDetect>();
            if (colliderDetect != null)
            {
                colliderDetect.enabled = false; // Disable the ColliderDetect script on each collision object to prevent it from counting collisions before the load is attached.
            }
        }
        
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
        UI.instance.UpdateScoreText("CollisionText", totalCollisions, "Collisions");
    }

    /// <summary>
    /// Enables or disables collision detection for all collision objects.
    /// </summary>
    /// <param name="enable"></param>
    void EnableCollisionDetection(bool enable)
    {
        foreach (GameObject obj in collisionObjects)
        {
            ColliderDetect colliderDetect = obj.GetComponent<ColliderDetect>();
            if (colliderDetect != null)
            {
                colliderDetect.enabled = enable;
            }
        }
    }
}