/*
    This script controls the swinging of the load (aka pivoting from a certain point).
    Created by: Claude
*/
using UnityEngine;

public class Pivot : MonoBehaviour
{
    // on the pivot that holds the load mesh
    Vector2 angle, angVel;
    [SerializeField] float stiffness = 20f, damping = 2f, kick = 0.15f, maxAngle = 15f;
    Vector3 lastPos, prevVelocity;
    
    void Start()
    {
        lastPos = transform.position;
    }
    void FixedUpdate()
    {
        Vector3 v = (transform.position - lastPos) / Time.fixedDeltaTime;
        lastPos = transform.position;
        Vector3 accel = (v - prevVelocity) / Time.fixedDeltaTime;
        prevVelocity = v;

        // horizontal acceleration only, in the pivot's parent space
        Vector3 localAccel = transform.parent != null
            ? transform.parent.InverseTransformDirection(accel)
            : accel;

        // sideways (x) swings around z, front/back (z) swings around x
        float fx = -stiffness * angle.x - damping * angVel.x - localAccel.x * kick;
        float fz = -stiffness * angle.y - damping * angVel.y + localAccel.z * kick;

        angVel.x += fx * Time.fixedDeltaTime;
        angVel.y += fz * Time.fixedDeltaTime;
        angle.x = Mathf.Clamp(angle.x + angVel.x * Time.fixedDeltaTime, -maxAngle, maxAngle);
        angle.y = Mathf.Clamp(angle.y + angVel.y * Time.fixedDeltaTime, -maxAngle, maxAngle);

        transform.localRotation = Quaternion.Euler(angle.y, 0f, angle.x);
    }
}
