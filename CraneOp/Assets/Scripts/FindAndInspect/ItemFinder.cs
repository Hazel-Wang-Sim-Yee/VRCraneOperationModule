/*
    This script is responsible for handling the item finding process in the game.
    It manages the interaction between the player and the items to be found, including enabling the trigger collider for each item, updating the UI with item details, and providing feedback when an item is found.
    The script also handles the highlighting of items when they are found and ensures that the appropriate UI
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class ItemFinder : MonoBehaviour
{
    [Header("Item Details")]
    public ItemFinderManager.ComponentItem data;

    [Header("Item Hover Settings")]
    [SerializeField] Renderer[] renderers;
    [SerializeField] Material[][] originals;   // original materials for each renderer
    XRSimpleInteractable interactable;

    public bool arrowUp;

    void Awake()
    {
        // Store the original materials for each renderer to allow for unhighlighting later
        originals = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originals[i] = renderers[i].sharedMaterials;
            }
            else
            {
                Debug.LogWarning("Renderer at index " + i + " is not assigned in the inspector.");
            }
        }
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            Debug.LogError("XRSimpleInteractable component not found on " + gameObject.name + ". Please ensure it is attached to the GameObject.");
        } else {
            interactable.enabled = false; // Disable the interactable at the start to prevent interaction before the item is loaded
        }
    }
    /// <summary>
    /// Initializes the item by enabling its trigger collider
    /// </summary>
    public void LoadItem()
    {
        foreach (Collider collider in data.itemTriggerColliders)
        {
            if (collider != null)
                collider.enabled = true; // Enable the trigger collider to allow interaction with the item
            else
                Debug.LogWarning("Trigger collider is not assigned for " + gameObject.name);
        }
        interactable.enabled = true; // Enable the interactable to allow interaction with the item

        // Check if the item name and defect images are set, and update the UI accordingly
        if (data.itemName == null || data.itemName == "" || data.itemDescription == null || data.itemDescription == "")
            Debug.LogWarning("Item name and/or description is not set for " + gameObject.name);
        if (ItemFinderManager.Instance.itemNameText != null)
            ItemFinderManager.Instance.itemNameText.text = data.itemName;
        else
            Debug.LogWarning("Please assign the UI reference for item name.");
        if (ItemFinderManager.Instance.itemDescText != null)
            ItemFinderManager.Instance.itemDescText.text = data.itemDescription;
        else
            Debug.LogWarning("Please assign the UI reference for item desc if needed.");
    }

    // Fallback methods if the player don't use raycast
    /// <summary>
    /// When the trainee is near the item, show the find button
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            // Check if the inspection panel is open, if it is, do not show the find button
            if (!ItemFinderManager.Instance.itemInspectionImage.gameObject.activeSelf)
                ItemFinderManager.Instance.findButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Fallback check to ensure that the find button was shown
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            // Check if the inspection panel is open, if it is, do not show the find button
            if (!ItemFinderManager.Instance.itemInspectionImage.gameObject.activeSelf)
                ItemFinderManager.Instance.findButton.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        ItemFinderManager.Instance.findButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// When the item is found, disable its trigger collider, give feedback and disable the script to prevent further interaction
    /// </summary>
    public void DisableItem()
    {
        // Disable the item's trigger collider to prevent further interaction
        foreach(Collider collider in data.itemTriggerColliders)
            if (collider != null)
                collider.enabled = false;

        interactable.enabled = false; // Disable the interactable to prevent further interaction

        // Change the item's material to the highlight material
        if (GetComponent<Renderer>() != null)
        GetComponent<Renderer>().material = ItemFinderManager.Instance.highlightMaterial;
        else
        Highlight(ItemFinderManager.Instance.highlightMaterial);
        ItemFinderManager.Instance.findButton.gameObject.SetActive(false);

        // Reset the defect images in the UI to null to clear the previous item's defects
        for (int i = 0; i < ItemFinderManager.Instance.images.Length; i++)
            ItemFinderManager.Instance.images[i].sprite = null;
        
        ItemFinderManager.Instance.UpdateChecklist();
        UI.instance.UpdateFeedbackText("Item completed: " + data.itemName);
        this.enabled = false;
    }

    /// <summary>
    /// Highlight all child of the component when found
    /// </summary>
    public void Highlight(Material highlightMaterial)
    {
        foreach (Renderer renderer in renderers)
            renderer.material = highlightMaterial;
    }

    public void Unhighlight()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            
            renderers[i].sharedMaterials = originals[i];
        }
    }
}