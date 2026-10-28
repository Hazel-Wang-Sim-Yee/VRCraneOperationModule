/*
    This script is responsible for handling the item finding process in the game.
    It manages the interaction between the player and the items to be found, including enabling/disabling items, updating the UI, and providing feedback when an item is found.
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class ItemFinder : MonoBehaviour
{
    public Collider itemTriggerCollider; // Reference for the item's trigger collider as there is 2 colliders on the item, one for the trigger and one for the colliding collider.
    public string itemName;
    [SerializeField] Sprite[] defectImages;

    /// <summary>
    /// Initializes the item by enabling its trigger collider
    /// </summary>
    public void LoadItem()
    {
        // Enable the item's trigger collider to allow the player to interact with it
        itemTriggerCollider.enabled = true;

        // Check if the item name and defect images are set, and update the UI accordingly
        if (itemName == null || itemName == "")
        {
            Debug.LogWarning("Item name is not set for " + gameObject.name);
        }
        else if (ItemFinderManager.Instance.itemNameText == null)
        {
            Debug.LogWarning("Item name text is not set in the ItemFinderManager.");
        }
        else
        {
            ItemFinderManager.Instance.itemNameText.text = itemName;
        }

        if (ItemFinderManager.Instance.defectImages == null || ItemFinderManager.Instance.defectImages.Length == 0)
        {
            Debug.LogWarning("Defect images are not set in the ItemFinderManager.");
        }
        else if (defectImages == null || defectImages.Length == 0)
        {
            Debug.LogWarning("Defect images are not set for " + gameObject.name);
        }
        else if (defectImages.Length != ItemFinderManager.Instance.defectImages.Length)
        {
            Debug.LogWarning("Defect images length does not match the number of defect image slots in the UI.");
        }
        else
        {
            for (int i = 0; i < defectImages.Length; i++)
            {
                ItemFinderManager.Instance.defectImages[i].sprite = defectImages[i];
            }
        }
    }

    /// <summary>
    /// When the trainee is near the item, show the find button
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        // Check if the inspection panel is open, if it is, do not show the find button
        if (!ItemFinderManager.Instance.itemInspectionImage.gameObject.activeSelf)
        {
            ItemFinderManager.Instance.findButton.gameObject.SetActive(true);
        }
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
        itemTriggerCollider.enabled = false;

        // Change the item's material to the highlight material
        GetComponent<Renderer>().material = ItemFinderManager.Instance.highlightMaterial;
        ItemFinderManager.Instance.findButton.gameObject.SetActive(false);

        // Reset the defect images in the UI to null to clear the previous item's defects
        for (int i = 0; i < ItemFinderManager.Instance.defectImages.Length; i++)
        {
            ItemFinderManager.Instance.defectImages[i].sprite = null;
        }

        ItemFinderManager.Instance.UpdateChecklist();
        UI.instance.UpdateFeedbackText("Item completed: " + itemName);
        this.enabled = false;
    }
}