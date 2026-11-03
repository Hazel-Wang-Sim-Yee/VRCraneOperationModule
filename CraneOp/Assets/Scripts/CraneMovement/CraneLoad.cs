/*
    This script manages the behavior of a liftable load in the game.
    It handles the lifting point, mass, and center of mass configuration.
    Put this on the root of any liftable load (regular or irregular). Needs a Rigidbody on the same object.
    Colliders can be on this object or on its children (compound collider for irregular shapes).
    Created by: Claude
    NOTE: This script is not in used in the current version of the game, but it is kept for future reference or potential use.
*/
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CraneLoad : MonoBehaviour
{
    [Header("Lifting")]
    [Tooltip("Empty child where the hook grabs the load (the lifting eye). Defaults to this object's pivot.")]
    [SerializeField] Transform liftingPoint;

    [Header("Physics")]
    [SerializeField] float mass = 5f;
    [Tooltip("Tick to set the centre of mass yourself (useful for lopsided loads). Otherwise Unity works it out from the colliders.")]
    [SerializeField] bool overrideCenterOfMass = false;
    [SerializeField] Vector3 localCenterOfMass;

    public Rigidbody Body { get; private set; }
    public Transform LiftingPoint => liftingPoint != null ? liftingPoint : transform;
    public bool HasLiftingPoint => liftingPoint != null;

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        Body.mass = mass;
        if (overrideCenterOfMass) Body.centerOfMass = localCenterOfMass;
    }

    // Right-click the component header > "Place lifting point above centre of mass".
    // Puts the lifting eye on top of the load, straight above its centre, so it hangs level.
    [ContextMenu("Place lifting point above centre of mass")]
    void PlaceLiftingPoint()
    {
        if (liftingPoint == null)
        {
            Debug.LogWarning("Assign a Lifting Point (empty child) first.", this);
            return;
        }
        Bounds b = GetBounds();
        Vector3 com = CenterOfMassWorld();
        liftingPoint.position = new Vector3(com.x, b.max.y, com.z);
    }

    Bounds GetBounds()
    {
        var cols = GetComponentsInChildren<Collider>();
        if (cols.Length == 0) return new Bounds(transform.position, Vector3.one);
        Bounds b = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
        return b;
    }

    Vector3 CenterOfMassWorld()
    {
        if (overrideCenterOfMass) return transform.TransformPoint(localCenterOfMass);
        if (Application.isPlaying && Body != null) return Body.worldCenterOfMass;
        return GetBounds().center; // rough guess in edit mode
    }

    // Select the load to see: yellow = lifting point, red = centre of mass, line between them.
    // A vertical line means the load hangs level; a slanted line means it will tilt.
    void OnDrawGizmosSelected()
    {
        Vector3 eye = LiftingPoint.position;
        Vector3 com = CenterOfMassWorld();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, 0.06f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(com, 0.06f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(eye, com);
    }
}