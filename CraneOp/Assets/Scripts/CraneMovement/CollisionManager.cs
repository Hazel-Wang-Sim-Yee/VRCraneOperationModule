/*
    This script is responsible for managing the collision detection of the crane operation module.
    It calculates the total number of collisions detected by all objects with the "CollisionObject" tag and updates the UI accordingly.
*/
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    int totalCollisions;
    [SerializeField] GameObject[] collisionObjects;
    void Start()
    {
        // Find all objects with the "CollisionObject" tag and store them in the collisionObjects array
        collisionObjects = GameObject.FindGameObjectsWithTag("CollisionObject");
        totalCollisions = 0;
    }
    void FixedUpdate()
    {
        CalculateTotalCollisions();
    }

    
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
}