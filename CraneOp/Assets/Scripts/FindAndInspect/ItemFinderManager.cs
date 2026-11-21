/*
    This script is responsible for managing the item finding process in the game.
    It keeps track of the items to be found, updates the UI, and provides feedback when an item is found.
    It also help to draw a line from the player to the current item to find.
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class ItemFinderManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] bool hasStartedFinding = false;
    [SerializeField] bool isFindingDefects = false; // Flag to indicate if the player is currently finding defects

    [Header("Singleton")]
    public static ItemFinderManager Instance { get; private set; }

    [Header("Item Finding Settings")]
    // Add all the items trainne has to find in the inspector
    public GameObject[] itemsToFind;
    public int currentItemIndex = 0;
    public Button findButton;
    public Image itemInspectionImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI header2Text;
    public Image[] images;
    public TextMeshProUGUI[] checklistItemsText;
    [System.Serializable]
    public class ComponentItem
    {
        public enum Categories { Defect, Information }

        public string itemName;
        [TextArea] public string itemDescription;
        public Categories category;
        public Sprite[] images;
        public List<string> possibleDefects;
        public string currentDefect;
        public List<Collider> itemTriggerColliders;
        public Transform arrowLoc;
        public Transform arrowLoc2;
    }

    [Header("Arrow Settings")]
    [SerializeField] float arrowLength;
    [SerializeField] float arrowPosition1;

    [Header("Highlight Settings")]
    public Material highlightMaterial;

    [Header("Defect Settings")]
    public Button[] defectButtons; // Array of buttons to be updated with possible defects for the current item

    [Header("Player Settings")]
    [SerializeField] GameObject player;
    [SerializeField] LineRenderer lineRenderer;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        // Disable all items at the start
        for (int i = 0; i < itemsToFind.Length; i++)
        {
            itemsToFind[i].GetComponent<ItemFinder>().enabled = false;
            foreach (Collider collider in itemsToFind[i].GetComponent<ItemFinder>().data.itemTriggerColliders)
                if (collider != null)
                    collider.enabled = false;
        }
        findButton.gameObject.SetActive(false);
        
        if (itemsToFind.Length == 0)
            Debug.LogWarning("No items to find!");
        else
        {
            //For each item to find, update the checklist items text with the item name
            for (int i = 0; i < checklistItemsText.Length; i++)
            {
                // Check if the current index is less than the number of items to find to avoid index out of range errors
                if (i < itemsToFind.Length)
                    checklistItemsText[i].text = i + 1 + ". " + itemsToFind[i].GetComponent<ItemFinder>().data.itemName;
                else
                    // If there are more checklist items than items to find, disable the extra checklist items
                    checklistItemsText[i].gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        StopAllCoroutines(); // Stop any existing coroutines to prevent conflicts

        // Check if the player and line renderer are assigned
        if (player == null)
            Debug.LogError("Player object not found. Please assign the player object in the inspector.");
        if (lineRenderer == null)
            Debug.LogError("LineRenderer component not found. Please assign the LineRenderer component in the inspector.");
    }

    void Update()
    {
        if (hasStartedFinding)
        {
            if ((currentItemIndex < itemsToFind.Length) && !itemInspectionImage.gameObject.activeSelf)
            {
                var item = itemsToFind[currentItemIndex];
                // Draw a line from the player to the current item to find
                lineRenderer.positionCount = 2;
                Transform arrowPosition = item.GetComponent<ItemFinder>().data.arrowLoc;
                Transform arrowPosition2 = item.GetComponent<ItemFinder>().data.arrowLoc2;

                lineRenderer.SetPosition (1, new Vector3(arrowPosition2.position.x, arrowPosition2.position.y, arrowPosition2.position.z));    
                lineRenderer.SetPosition(0, new Vector3(arrowPosition.position.x, arrowPosition.position.y, arrowPosition.position.z));
            }
            else
                lineRenderer.positionCount = 0; // Disable the line renderer when all items have been found
        } else
            lineRenderer.positionCount = 0; // Disable the line renderer
    }

    /// <summary>
    /// Updates the current item to find
    /// </summary>
    public void UpdateCurrentItem()
    {
        if (!hasStartedFinding)
            hasStartedFinding = true;
        itemsToFind[currentItemIndex].GetComponent<ItemFinder>().enabled = true;

        // Load the defects for the current item if the player is finding defects
        if (isFindingDefects)
            LoadDefects();

        LoadImages();

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
            UI.Instance.UpdateFeedbackText(isFindingDefects? "All defects identified!":"All items found!");
            StartCoroutine(DelayToNextModule(3f));
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

    /// <summary>
    /// Delays the transition to the next module after a specified delay to ensure that the final feedback text is visible to the user.
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator DelayToNextModule(float delay)
    {
        yield return new WaitForSeconds(delay);
        Events.Instance.SequenceComplete();
    }
    
    /// <summary>
    /// Load the images for the current item into the UI
    /// </summary>
    void LoadImages()
    {
        var currentItem = itemsToFind[currentItemIndex].GetComponent<ItemFinder>().data;
        // Load the defect images for the current item into the defect images in the UI
        if (currentItem.images.Length == 0)
        {
            Debug.LogWarning("No defect images found for item: " + currentItem.itemName);
            return;
        }
        else if (currentItem.images.Length > images.Length)
        {
            Debug.LogWarning("Not enough defect image slots for item: " + currentItem.itemName);
            return;
        }
        for (int i = 0; i < images.Length; i++)
        {
            // When the current index is less than the number of defect images for the current item, enable the image and set its sprite to the defect image
            if (i < currentItem.images.Length)
            {
                images[i].gameObject.SetActive(true);
                images[i].sprite = currentItem.images[i];
                images[i].preserveAspect = true;
            }
            // Disable any remaining defect images if there are fewer defect images than UI slots
            else
                images[i].gameObject.SetActive(false);
        }
        
        if (!isFindingDefects)
        {
            if (header2Text == null)
                Debug.LogWarning("Header2 text is not set in the ItemFinderManager.");
            else
            {
                if (currentItem.category == ItemFinderManager.ComponentItem.Categories.Defect)
                    header2Text.text = "Possible Defects";
                else if (currentItem.category == ItemFinderManager.ComponentItem.Categories.Information)
                    header2Text.text = "Component Information";
            }
        }
    }
    /// <summary>
    /// Loads the defects for the current item into the defect buttons in the UI
    /// </summary>
    void LoadDefects()
    {
        var currentItem = itemsToFind[currentItemIndex].GetComponent<ItemFinder>().data;
        
        // Validation checks to ensure that the current item has possible defects and that there are enough defect buttons to display them
        if (currentItem.possibleDefects.Count == 0)
        {
            Debug.LogWarning("No defects found for item: " + currentItem.itemName);
            return;
        }
        else if (currentItem.possibleDefects.Count > defectButtons.Length)
        {
            Debug.LogWarning("Not enough defect buttons for item: " + currentItem.itemName);
            return;
        }
        else if (currentItem.currentDefect == null || currentItem.currentDefect == "")
        {
            Debug.LogWarning("Current defect not set for item: " + currentItem.itemName);
            return;
        }

        // Load the defects for the current item into the defect buttons in the UI
        for (int i = 0; i < defectButtons.Length; i++)
        {
            // When the current index is less than the number of possible defects for the current item, enable the button and set its text to the defect name
            if (i < currentItem.possibleDefects.Count)
            {
                defectButtons[i].gameObject.SetActive(true);
                defectButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentItem.possibleDefects[i];

                // Add a listener to the button to check if the selected defect is correct
                if (currentItem.possibleDefects[i] == currentItem.currentDefect)
                {
                    defectButtons[i].onClick.RemoveAllListeners();
                    defectButtons[i].onClick.AddListener(() => {DefectSelection(true); });
                }
                else
                {
                    defectButtons[i].onClick.RemoveAllListeners();
                    defectButtons[i].onClick.AddListener(() => {DefectSelection(false); });
                }
            }
            // Disable any remaining defect buttons if there are fewer defects than buttons
            else
                defectButtons[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the selection of a defect by the player.
    /// If the selected defect is correct, it provides positive feedback and proceeds to the next item. 
    /// f incorrect, it provides negative feedback and allows the player to try again.
    /// </summary>
    /// <param name="isCorrect"></param>
    void DefectSelection(bool isCorrect)
    {
        if (isCorrect)
        {
            Events.Instance.ItemCorrect();
            itemInspectionImage.gameObject.SetActive(false);
            OnItemFound();
        }
        else
            Events.Instance.ItemWrong();
    }
}