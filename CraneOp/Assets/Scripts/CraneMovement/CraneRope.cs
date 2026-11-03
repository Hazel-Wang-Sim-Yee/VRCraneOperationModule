/*
    This script manages the behavior of a single crane rope in the game.
    It handles the visual representation of the rope and its physical properties.
    Attach this script to each rope and assign the appropriate anchor and hook bodies, as well as the visual top if needed.
    Created by: Claude
*/
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CraneRope : MonoBehaviour
{
    [Header("Bodies")]
    [Tooltip("The kinematic attach Rigidbody (e.g. leftAttachRb) the rope hangs from.")]
    [SerializeField] Rigidbody anchorBody;
    [Tooltip("Where the rope leaves the attach point. Defaults to the body's transform.")]
    [SerializeField] Transform anchorPoint;
    [SerializeField] Rigidbody hookBody;
    [Tooltip("Where this rope joins the hook (one empty child on each side of the hook).")]
    [SerializeField] Transform hookAnchor;
    [Tooltip("Optional. Where the rope is DRAWN from, e.g. an empty at the drum on the trolley. Leave empty to draw from the anchor point.")]
    [SerializeField] Transform visualTop;

    [Header("Length")]
    [Tooltip("Use the distance between anchor and hook at start as the rope length (recommended).")]
    [SerializeField] bool keepCurrentLength = true;
    [SerializeField] float ropeLength = 5f;

    [Header("Look")]
    [SerializeField] int segments = 12;
    [SerializeField] float sagAmount = 0.5f;

    [Header("Swing damping (applied to the hook)")]
    [SerializeField] float hookDrag = 0.2f;
    [SerializeField] float hookAngularDrag = 1f;

    ConfigurableJoint joint;
    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        if (anchorPoint == null) anchorPoint = anchorBody.transform;

        if (keepCurrentLength)
            ropeLength = Vector3.Distance(anchorPoint.position, hookAnchor.position);

        anchorBody.interpolation = RigidbodyInterpolation.Interpolate;
        hookBody.linearDamping = hookDrag;           // use .drag on Unity 2022 or older
        hookBody.angularDamping = hookAngularDrag;   // use .angularDrag on Unity 2022 or older
        hookBody.interpolation = RigidbodyInterpolation.Interpolate;

        // Hook shouldn't collide with the attach point it hangs from.
        foreach (var a in hookBody.GetComponentsInChildren<Collider>())
            foreach (var b in anchorBody.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(a, b, true);

        joint = hookBody.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = anchorBody;
        joint.anchor = hookBody.transform.InverseTransformPoint(hookAnchor.position);
        joint.connectedAnchor = anchorBody.transform.InverseTransformPoint(anchorPoint.position);

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        ApplyLength();
    }

    void ApplyLength()
    {
        var limit = joint.linearLimit;
        limit.limit = ropeLength;
        limit.bounciness = 0f;
        joint.linearLimit = limit;
    }

    void LateUpdate()
    {
        Vector3 a = visualTop != null ? visualTop.position : anchorPoint.position;
        Vector3 b = hookAnchor.position;
        // Slack still comes from the physics anchor, so the sag looks right.
        float slack = Mathf.Max(0f, ropeLength - Vector3.Distance(anchorPoint.position, hookAnchor.position));

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = Vector3.Lerp(a, b, t);
            // Parabola: sag is biggest in the middle and only shows when the rope is slack.
            p.y -= 4f * t * (1f - t) * slack * sagAmount;
            line.SetPosition(i, p);
        }
    }
}