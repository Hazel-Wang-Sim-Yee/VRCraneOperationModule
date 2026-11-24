using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public enum GearType {Shackle, Hook, Sling, Other};
public class GearSettings : MonoBehaviour
{
    public GearType gearType;
    public TextMeshProUGUI text;
    XRGrabInteractable grab;
    Transform primaryAttachPoint;
    [SerializeField] Transform secondaryAttachPoint;


    void Start()
    {
        if (text != null) text.text = $"{gameObject.name}";
        grab = GetComponent<XRGrabInteractable>();
        primaryAttachPoint = grab.attachTransform;   // remember the original
    }
    
    void OnEnable()
    {
        Events.Instance.onLoadLoaded += DisableText;
        Events.Instance.onLoadUnloaded += EnableText;
    }
    void OnDisable()
    {
        Events.Instance.onLoadLoaded -= DisableText;
        Events.Instance.onLoadUnloaded -= EnableText;
    }

    /// <summary>
    /// Change the attachment point to the one for attaching onto the crane.
    /// </summary>
    public void SwapAttachmentPoint()
    {
        if (secondaryAttachPoint == null) return;
        grab.attachTransform = secondaryAttachPoint;
    }

    /// <summary>
    /// Change back the attachment point to the original one
    /// </summary>
    public void ResetAttachmentPoint() => grab.attachTransform = primaryAttachPoint;

    // Methods to allow easy subscribing and unsubscribing
    void EnableText(){
        if (text == null) return;
        text.enabled = true;
    }
    void DisableText(){
        if (text == null) return;
        text.enabled = false;
    }
}
