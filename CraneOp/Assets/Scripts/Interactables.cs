/*
    This script handles the interaction of the pendant which includes rescaling and locking/unlocking of the interaction.
    This script can be expanded to other uses if needed.
*/
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Interactables : MonoBehaviour
{
    [SerializeField] Transform interactable;

    void Start()
    {
        if (interactable == null)
        {
            Debug.LogWarning("Interactable Transform not assigned. Please assign a Transform in the inspector.");
        }
    }

    /// <summary>
    /// Locks or unlocks the grab interaction of the pendant by changing its interaction layer mask.
    /// This is to allow the trainee to interact with the buttons without accidentally grabbing the pendant when they are using the pendant.
    /// </summary>
    /// <param name="lockInteraction"></param>
    public void LockInteraction(bool lockInteraction)
    {
        if (interactable != null)
        {
            XRGrabInteractable grabInteract = interactable.GetComponent<XRGrabInteractable>();
            if (grabInteract != null)
            {
                grabInteract.interactionLayers = lockInteraction ? InteractionLayerMask.GetMask("LockedPendant") : InteractionLayerMask.GetMask("Pendant");
            }
            else
            {
                Debug.LogWarning("XRGrabInteractable component not found on the interactable. Please ensure it is attached to the Transform.");
            }
        }
        else
        {
            Debug.LogWarning("Interactable Transform is null. Cannot lock interaction.");
        }
    }

    /// <summary>
    /// Rescales the interactable object and all its children by the specified scale factor.
    /// </summary>
    /// <param name="scaleFactor"></param>
    public void Rescale(float scaleFactor)
    {
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }
}
