/*
    This script manages the behavior of the load hook in the game.
    It handles the attachment and detachment of loads, as well as the joint configuration and collision management.
    Created by: Claude
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadHookSettings : MonoBehaviour
{
    [Header("References (auto-found by name if left empty)")]
    [SerializeField] Rigidbody hookBody;   // the HOOK's Rigidbody, not the socket's
    [SerializeField] Transform loadPos;
    [SerializeField] GameObject load;
    [Tooltip("Parent of the hook, socket and all rope segments. Their colliders are ignored by the load while attached.")]
    [SerializeField] Transform craneRoot;

    [Header("Capacity")]
    [Tooltip("Heaviest load (kg) the crane can lift. Heavier loads are refused and the player is told.")]
    [SerializeField] float ratedCapacityKg = 50f;

    [Header("Joint")]
    [Tooltip("Locked = rigid hold. Untick to let the load swing.")]
    [SerializeField] bool lockRotation = true;
    [Tooltip("Load is made to feel at most this many times heavier than the hook inside the joint.")]
    [SerializeField] float maxMassRatio = 5f;
    [SerializeField] int solverIterations = 40;
    [SerializeField] float collisionRestoreDelay = 0.75f;

    bool loadWasKinematic;

    ConfigurableJoint joint;
    Rigidbody loadRb;
    readonly List<(Collider a, Collider b)> ignored = new List<(Collider, Collider)>();

    CollisionManager collisionManager;

    void Awake()
    {
        // Get the hookBody from the socket parent (hook) rigidbody if not set in the inspector.
        if (hookBody == null) hookBody = GetComponentInParent<Rigidbody>();
        if (hookBody == null) Debug.LogError("No Rigidbody found on the hook (parent of the socket).", this);

        // Find loadPos from the scene if not set in the inspector. If found, set the loadPos to the load's position.
        if (loadPos == null)
        {
            var go = GameObject.Find("loadPos");
            if (go != null) loadPos = go.transform;
            else Debug.LogError("loadPos GameObject not found in the scene.", this);
        }

        // craneRoot is the parent of the entire crane (hook, socket, ropes). If not set in the inspector, find it from the hookBody's root.
        if (craneRoot == null && hookBody != null) craneRoot = hookBody.transform.root;
    }

    void Start()
    {
        collisionManager = GameObject.FindObjectOfType<CollisionManager>();
        if (collisionManager == null)
        {
            Debug.LogError("CollisionManager script not found in the scene. Please ensure that it is in the scene if you require load collision management functionality.");
        }
    }

    /// <summary>
    /// True if the given load is light enough for the crane. Other scripts (e.g. whatever
    /// detects a load entering the socket) can call this before attaching.
    /// </summary>
    public bool IsWithinCapacity(GameObject candidate)
    {
        if (candidate == null) return true;
        var rb = candidate.GetComponent<Rigidbody>();
        return rb == null || rb.mass <= ratedCapacityKg;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Load"))
        {
            load = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Load") && other.gameObject == load)
        {
            load = null;
        }
    }
    
    /// <summary>
    /// Set up joints and collision ignores so the load is attached to the hook.
    /// </summary>
    public void LoadLoaded()
    {
        // Check if the load and hook are set up correctly before proceeding with the joint creation.
        if (load == null || hookBody == null)
        {
            Debug.LogError("LoadLoaded: missing load or hook Rigidbody.", this);
            return;
        }

        // If a joint already exists, do not create another one.
        if (joint != null) return;

        // Too heavy: tell the player and don't attach anything.
        if (!IsWithinCapacity(load))
        {
            float mass = load.GetComponent<Rigidbody>().mass;
            if (UI.instance != null)
                UI.instance.UpdateFeedbackText(
                    $"This load is too heavy to lift ({mass:0.#} kg). The crane's limit is {ratedCapacityKg:0.#} kg."
                );
            return;
        }

        StopAllCoroutines();   // Make sure the RestoreCollisionsLater coroutine is not running when attaching a new load.
        if (loadPos != null) loadPos.position = load.transform.position; // Set the loadPos to the load's position

        // Get or add a Rigidbody to the load if it doesn't have one.
        loadRb = load.GetComponent<Rigidbody>();
        if (loadRb == null) loadRb = load.AddComponent<Rigidbody>();

        // Reset the load's Rigidbody properties
        loadRb.linearVelocity = Vector3.zero;
        loadRb.angularVelocity = Vector3.zero;
        loadRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        loadRb.interpolation = RigidbodyInterpolation.Interpolate;

        // Set up the solver iterations for the load and hook Rigidbody to ensure stable physics simulation.
        loadRb.solverIterations = solverIterations;
        loadRb.solverVelocityIterations = solverIterations / 4;
        hookBody.solverIterations = solverIterations;
        hookBody.solverVelocityIterations = solverIterations / 4;

        // Creates a ConfigurableJoint to attach the load to the hook. The joint is configured to lock the position and optionally lock the rotation, depending on the lockRotation setting.
        joint = load.AddComponent<ConfigurableJoint>();
        joint.connectedBody = hookBody;

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        var rot = lockRotation ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Free;
        joint.angularXMotion = rot;
        joint.angularYMotion = rot;
        joint.angularZMotion = rot;

        // Calculate the allowed mass for the joint. If the load's mass exceeds the allowed mass, the joint's mass scale is set to 1, otherwise it is set to the ratio of the allowed mass to the load's mass.
        float allowed = hookBody.mass * maxMassRatio;
        joint.massScale = loadRb.mass > allowed ? allowed / loadRb.mass : 1f;
        joint.connectedMassScale = 1f;

        // Ensure that the joint does not break due to excessive forces by setting the projection mode and limits.
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.01f;
        joint.projectionAngle = 1f;
        loadWasKinematic = loadRb.isKinematic;
        loadRb.isKinematic = false;

        // Set up collision objects for detection and management while the load is attached.
        // The CollisionManager script is enabled to manage collisions between the load and the crane, and the ColliderDetect script on each collision object is enabled to allow it to count collisions while the load is attached.
        SignalmanManager.instance.isLoadAttached = true; // Set the isLoadAttached flag to true when the load is attached.
        collisionManager.enabled = true; // Enable the CollisionManager script to manage collisions between the load and the crane while the load is attached.
        foreach(GameObject obj in collisionManager.collisionObjects)
        {
            ColliderDetect colliderDetect = obj.GetComponent<ColliderDetect>();
            if (colliderDetect != null)
            {
                colliderDetect.enabled = true; // Enable the ColliderDetect script on each collision object to allow it to count collisions while the load is attached.
            }
        }
        SignalmanManager.instance.targets[SignalmanManager.instance.currentSignalIndex].SetActive(true); // Activate the current target area for the signalman to guide the crane operator.
        Events.Instance.LoadLoaded();
    }

    /// <summary>
    /// Remove the joint and restore collisions so the load is detached from the hook.
    /// </summary>
    public void LoadUnloaded()
    {
        if (joint != null)
        {
            Destroy(joint);
            if (loadRb != null)
            {
                loadRb.isKinematic = false;   // let physics take over so it falls / rests
                loadRb.WakeUp();
            }
            joint = null;
        }

        if (loadPos != null && hookBody != null)
            loadPos.position = hookBody.transform.position;

        // Give the load time to clear the hook before collisions come back.
        StartCoroutine(RestoreCollisionsLater());

        // Disable the ColliderDetect script on each collision object to prevent it from counting collisions after the load is detached.
        SignalmanManager.instance.isLoadAttached = false; // Set the isLoadAttached flag to false when the load is detached.
        foreach(GameObject obj in collisionManager.collisionObjects)
        {
            ColliderDetect colliderDetect = obj.GetComponent<ColliderDetect>();
            if (colliderDetect != null)
            {
                colliderDetect.enabled = false; // Disable the ColliderDetect script on each collision object to prevent it from counting collisions after the load is detached.
            }
        }
        collisionManager.enabled = false; // Disable the CollisionManager script to prevent collisions detection after the load is detached.
        SignalmanManager.instance.targets[SignalmanManager.instance.currentSignalIndex].SetActive(false); // Deactivate the current target area for the signalman since the load has been unloaded.
        Events.Instance.LoadUnloaded();
    }

    /// <summary>
    /// Set up the collision ignores between the load and the crane (hook, socket, ropes) while the load is attached.
    /// When the load is detached, restore collisions after a short delay to avoid immediate collisions.
    /// </summary>
    /// <param name="ignore"></param>
    void IgnoreCrane(bool ignore)
    {
        if (ignore)
        {
            ignored.Clear();
            var loadCols = load.GetComponentsInChildren<Collider>();
            var craneCols = craneRoot.GetComponentsInChildren<Collider>();
            foreach (var a in loadCols)
                foreach (var b in craneCols)
                {
                    if (a == b || b.transform.IsChildOf(load.transform)) continue;
                    Physics.IgnoreCollision(a, b, true);
                    ignored.Add((a, b));
                }
        }
        else
        {
            foreach (var (a, b) in ignored)
                if (a != null && b != null) Physics.IgnoreCollision(a, b, false);
            ignored.Clear();
        }
    }

    /// <summary>
    /// Restore collisions between the load and the crane after a short delay to avoid immediate collisions when the load is detached.
    /// </summary>
    /// <returns></returns>
    IEnumerator RestoreCollisionsLater()
    {
        yield return new WaitForSeconds(collisionRestoreDelay);
        IgnoreCrane(false);
    }

    void FixedUpdate()
    {
        if (load != null && joint != null)
        {
            // Keep the loadPos at the load's position while the load is attached.
            if (loadPos != null) loadPos.position = load.transform.position;
        }
    }
}