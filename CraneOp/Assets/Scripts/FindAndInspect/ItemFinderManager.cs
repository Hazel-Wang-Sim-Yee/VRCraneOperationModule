/*
    This script is responsible for managing the item finding process in the game.
    It keeps track of the items to be found, updates the UI, and provides feedback when an item is found.
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ItemFinderManager : MonoBehaviour
{
    public static ItemFinderManager Instance { get; private set; }
    // Add all the items trainne has to find in the inspector
    public GameObject[] itemsToFind;
    public int currentItemIndex = 0;
    public Button findButton;
    [SerializeField] Button nextButton;
    public Image itemInspectionImage;
    public Material highlightMaterial;
    public TextMeshProUGUI itemNameText;
    public Image[] defectImages;
    public TextMeshProUGUI[] checklistItemsText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Disable all items at the start
        for (int i = 0; i < itemsToFind.Length; i++)
        {
            itemsToFind[i].GetComponent<ItemFinder>().enabled = false;
            itemsToFind[i].GetComponent<ItemFinder>().itemTriggerCollider.enabled = false;
        }
        findButton.gameObject.SetActive(false);

        // Check if the checklist items text length matches the number of items to find
        if (itemsToFind.Length != checklistItemsText.Length)
        {
            Debug.LogWarning("Checklist items text length does not match the number of items to find.");
        }
        else if (itemsToFind.Length == 0)
        {
            Debug.LogWarning("No items to find!");
        }
        else
        {
            // If the lengths match, set the checklist items text to the names of the items to find
            for (int i = 0; i < itemsToFind.Length; i++)
            {
                checklistItemsText[i].text = i+1 + ". " + itemsToFind[i].GetComponent<ItemFinder>().itemName;
            }
            // Enable the first item to find
        }
    }

    /// <summary>
    /// Updates the current item to find
    /// </summary>
    public void UpdateCurrentItem()
    {
        itemsToFind[currentItemIndex].GetComponent<ItemFinder>().enabled = true;
        itemsToFind[currentItemIndex].GetComponent<ItemFinder>().LoadItem();
    }

    /// <summary>
    /// Disables the current item and moves on to the next item in the list.
    /// If all items have been found, it provides feedback to the user.
    /// </summary>
    public void OnItemFound()
    {
        itemsToFind[currentItemIndex].GetComponent<ItemFinder>().DisableItem();
        currentItemIndex++;
        if (currentItemIndex >= itemsToFind.Length)
        {
            UI.instance.UpdateFeedbackText("All items found!");
            nextButton.gameObject.SetActive(true);
            return;
        }
        UpdateCurrentItem();
    }

    /// <summary>
    /// Updates the checklist to indicate that the item has been found
    /// </summary>
    public void UpdateChecklist()
    {
        checklistItemsText[currentItemIndex].text = "<s>" + checklistItemsText[currentItemIndex].text + "</s>";
    }
}