/*
    This script is responsible for handling the item finding process in the game.
    It enables the item's trigger collider, shows the find button when the player is near the item, and disables the item when found.
    Note: You may delete this script if you do not want to use the item finding feature in your game.
*/
using UnityEngine;

public class ItemFinder : MonoBehaviour
{
    public Collider itemTriggerCollider; // Reference for the item's trigger collider as there is 2 colliders on the item, one for the trigger and one for the colliding collider.

    /// <summary>
    /// Initializes the item by enabling its trigger collider
    /// </summary>
    public void LoadItem()
    {
        itemTriggerCollider.enabled = true;
        Debug.Log("Item loaded!");
    }

    /// <summary>
    /// When the trainee is near the item, show the find button
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
        GetComponent<Renderer>().material = ItemFinderManager.Instance.highlightMaterial;
        itemTriggerCollider.enabled = false;
        ItemFinderManager.Instance.findButton.gameObject.SetActive(false);
        this.enabled = false;
    }
}