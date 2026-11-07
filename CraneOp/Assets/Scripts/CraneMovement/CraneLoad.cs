/*
    This script manages the behavior of a liftable load in the game.
    It handles the lifting point, mass, and center of mass configuration.
    Put this on the root of any liftable load (regular or irregular). Needs a Rigidbody on the same object.
    Colliders can be on this object or on its children (compound collider for irregular shapes).
    Created by: Claude
    NOTE: Only line renderer functionality is currently implemented.
    Other features like mass and center of mass configuration are present but not used in the current game logic.
*/
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class CraneLoad : MonoBehaviour
{
    [Header("Load Settings")]
    [Tooltip("The type of load.")]
    [SerializeField] LoadType loadType;
    enum LoadType { Regular, Irregular, Unbalanced }

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

    [Header("Line Renderer (auto found if left empty)")]
    [SerializeField] LineRenderer lineRenderer;
    bool drawLine = false; // Flag to indicate whether to draw the line to the target area

    [Header("UI Settings")]
    [Tooltip("The canvas to display load information (e.g., weight, type).")]
    [SerializeField] Canvas loadInfoCanvas; // Canvas to display load information (e.g., weight, type)

    [Header("Script References")]
    [SerializeField] LoadHookSettings loadHookSettings;

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        Body.mass = mass;
        if (overrideCenterOfMass) Body.centerOfMass = localCenterOfMass;
    }

    void Start()
    {
        Events.Instance.onLoadLoaded += () => {drawLine = true; loadInfoCanvas.gameObject.SetActive(false); lineRenderer.enabled = true;};
        Events.Instance.onLoadUnloaded += () => {drawLine = false; loadInfoCanvas.gameObject.SetActive(true); lineRenderer.enabled = false;};

        if (loadInfoCanvas != null)
        {
            loadInfoCanvas.GetComponentInChildren<TextMeshProUGUI>().text = $"Type: {loadType}\nWeight: {mass:0.#} kg";
        }
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.LogWarning("LineRenderer component not found on the load. Please add a LineRenderer component to visualize the line to the target.");
            }
        }
        if (loadHookSettings == null)
        {
            loadHookSettings = FindObjectOfType<LoadHookSettings>();
            if (loadHookSettings == null)
            {
                Debug.LogError("LoadHookSettings script not found in the scene. Please ensure it is attached to a GameObject.");
            }
        }
    }

    void LateUpdate()
    {
        if (loadHookSettings.load == this.gameObject)
        {
            if (SignalmanManager.instance.currentSignalIndex < SignalmanManager.instance.targets.Length)
            {
                if (drawLine && lineRenderer != null)
                {
                    DrawLineToTarget(SignalmanManager.instance.targets[SignalmanManager.instance.currentSignalIndex].transform.position);
                } else if (!drawLine && lineRenderer != null)
                {
                    lineRenderer.positionCount = 0; // Clear the line when not drawing
                }
            } else 
            {
                if (lineRenderer != null)
                {
                    lineRenderer.positionCount = 0; // Load reached the last target, clear the line
                }
            }
        }
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

    /// <summary>
    /// Draws a line from the load's position to the specified target position using the LineRenderer component.
    /// This helps the player to see the direction to the target area.
    /// </summary>
    /// <param name="target"></param>
    public void DrawLineToTarget(Vector3 target)
    {
        if (lineRenderer == null) return;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, target);
    }
}