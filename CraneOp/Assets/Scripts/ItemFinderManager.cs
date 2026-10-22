/*
    This script is responsible for managing the item finding process in the game.
    It keeps track of the current item to find, enables/disables items, and handles the logic when an item is found.
    Note: You may delete this script if you do not want to use the item finding feature in your game.
*/
using UnityEngine;
using UnityEngine.UI;

public class ItemFinderManager : MonoBehaviour
{
    public static ItemFinderManager Instance { get; private set; }
    // Add all the items trainne has to find in the inspector
    public GameObject[] itemsToFind;
    int currentItemIndex = 0;
    public Button findButton;
    public Material highlightMaterial;

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

        // Enable the first item to find
        UpdateCurrentItem();

        if (itemsToFind.Length == 0)
        {
            Debug.LogWarning("No items to find!");
        }
    }

    /// <summary>
    /// Updates the current item to find
    /// </summary>
    void UpdateCurrentItem()
    {
        itemsToFind[currentItemIndex].GetComponent<ItemFinder>().enabled = true;
        itemsToFind[currentItemIndex].GetComponent<ItemFinder>().LoadItem();
    }

    /// <summary>
    /// Called when an item is found
    /// </summary>
    public void OnItemFound()
    {
        itemsToFind[currentItemIndex].GetComponent<ItemFinder>().DisableItem();
        currentItemIndex++;
        if (currentItemIndex >= itemsToFind.Length)
        {
            Debug.Log("All items found!");
            // Add UI or other feedback to indicate that all items have been found
            return;
        }
        UpdateCurrentItem();
    }
}