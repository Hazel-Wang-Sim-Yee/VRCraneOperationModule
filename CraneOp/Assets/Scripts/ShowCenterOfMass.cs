/*
    This script helps you see the center of mass of a Rigidbody in the Unity Editor.
    It draws a red sphere at the center of mass position, a yellow line from the pivot to the center of mass, and a blue wireframe sphere at the pivot position.
    Created by: ChatGPT
*/
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShowCenterOfMass : MonoBehaviour
{
    public float markerSize = 0.12f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnDrawGizmos()
    {
        Rigidbody body = GetComponent<Rigidbody>();

        if (body == null) return;

        // Get the actual world position of the center of mass
        Vector3 comPosition = body.worldCenterOfMass;

        // Draw center of mass
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(comPosition, markerSize);

        // Draw line from pivot to center of mass
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, comPosition);

        // Draw pivot marker
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, markerSize * 0.5f);
    }
}