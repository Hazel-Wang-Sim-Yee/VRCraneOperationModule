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

    public void SwapAttachmentPoint()
    {
        if (secondaryAttachPoint == null) return;
        grab.attachTransform = secondaryAttachPoint;
    }

    public void ResetAttachmentPoint() => grab.attachTransform = primaryAttachPoint;
}
