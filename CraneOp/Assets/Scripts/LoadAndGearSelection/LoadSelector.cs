/*
    This script is responsible for handling the load and gear selection process.
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Rendering;

public class LoadSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] List<Button> loadOptions;
    [SerializeField] TextMeshProUGUI selectedOptionText;
    [SerializeField] TextMeshProUGUI gearSelectionText;
    [SerializeField] List<GameObject> requiredGearObjects;
    [SerializeField] List<Image> requiredGearImages;
    [SerializeField] List<TextMeshProUGUI> requiredGearNames;

    [Header("Load")]
    [SerializeField] List<LoadDetails> allLoads;
    [System.Serializable]
    public class LoadDetails
    {
        public string loadName;
        public GameObject loadPrefab;
        public List<Sprite> gearSprites;
        public List<string> gearNames;
    }

    [Header("Load Tracking")]
    string currentSelectedOption = null;
    List<string> gearsToCheck;
    int numberOfGearsFound;
    [Tooltip("Put all the gears here")]
    [SerializeField] List<XRGrabInteractable> allGears;
    [SerializeField] List<XRSocketInteractor> gearSockets;
    List<XRGrabInteractable> placedGears;
    [SerializeField] Transform loadSpawnPos;
    XRGrabInteractable rejectedGear;

    [Header("Things To Unlock")]
    [SerializeField] List<GameObject> disables;
    [SerializeField] List<GameObject> enables;
    public LineRenderer navigation;
    Coroutine waitingCoroutine;
    [SerializeField] LoadHookSettings loadHookSettings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Validation checks and initialisations
        if (loadOptions == null || loadOptions.Count == 0) Debug.LogError("Please assign load options");
        if (selectedOptionText == null) Debug.LogWarning("Select Option Text is null. Please assign it in the inspector.");
        if (gearSelectionText == null) Debug.LogWarning("Gear Select Text is null. Please assign it in the inspector.");
        if (requiredGearImages == null || requiredGearImages.Count == 0) Debug.LogWarning("Gear images array is null. Please add images in the inspector");
        if (requiredGearNames == null || requiredGearNames.Count == 0) Debug.LogWarning("Gear names array is null. Please add names in the inspector.");
        if (requiredGearObjects == null || requiredGearObjects.Count == 0) Debug.LogWarning("Please assign the gameobjects of gear images and gear names.");
        else
        {
            foreach (GameObject obj in requiredGearObjects) obj.SetActive(false);
        }
        foreach (LoadDetails load in allLoads)
        {
            if (load.loadName == "") Debug.LogWarning("Give the load a name.");
            if (load.loadPrefab == null) Debug.LogWarning($"{load.loadName} prefab doesn't exist");
            if (load.gearSprites == null || load.gearSprites.Count == 0) Debug.LogWarning($"Assign gears for the {load.loadName}.");
            if (load.gearNames == null || load.gearNames.Count == 0) Debug.LogWarning("Name the gears.");
        }
        if (selectedOptionText != null) selectedOptionText.text = "Select a load.";
        if (placedGears == null) placedGears = new List<XRGrabInteractable>();
        if (gearsToCheck == null) gearsToCheck = new List<string>();
        foreach (XRSocketInteractor socket in gearSockets) socket.socketActive = false;
    }

    /// <summary>
    /// Select an option and toggle the interactable states of the other options.
    /// </summary>
    /// <param name="optionName"></param>
    public void ChooseOption(string optionName)
    {
        // validate: the name must match one of the buttons
        if (!loadOptions.Exists(b => b != null && b.name == optionName))
        {
            Debug.LogWarning($"LoadSelector: '{optionName}' is not a valid option.", this);
            return;
        }

        // Check if a correct gear is already placed. If it is, don't allow them to change load.
        if (placedGears != null  && placedGears.Count > 0)
        {
            Debug.LogWarning($"Options are locked.");
            return;
        }

        if (string.IsNullOrEmpty(currentSelectedOption))
        {
            // lock every other option
            currentSelectedOption = optionName;
            foreach (Button b in loadOptions)
                b.interactable = (b.name == optionName);
            gearSelectionText.gameObject.SetActive(false);
            LoadReqGears(optionName);
        }
        else if (currentSelectedOption == optionName)
        {
            // unlock all options
            currentSelectedOption = null;
            foreach (Button b in loadOptions)
                b.interactable = true;
            gearSelectionText.gameObject.SetActive(true);
            foreach(GameObject obj in requiredGearObjects) obj.SetActive(false);
            gearsToCheck.Clear();
            placedGears.Clear();
            numberOfGearsFound = 0;
        }

        LoadDetails details = GetLoad(currentSelectedOption);
        selectedOptionText.text = currentSelectedOption == null
            ? "Select a load."
            : $"Current Selection: {currentSelectedOption}";
        rejectedGear = null;
    }

    /// <summary>
    /// Check if the load exists and gets its reference if it does.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    LoadDetails GetLoad(string name)
    {
        foreach (LoadDetails load in allLoads)
        {
            if (load.loadName == name) return load;
        }
        return null;
    }

    /// <summary>
    /// Load and show the relevant gears required for the load if the load exists and has data.
    /// </summary>
    /// <param name="optionName"></param>
    void LoadReqGears(string optionName)
    {
        // hide everything first
        foreach(GameObject obj in requiredGearObjects) obj.SetActive(false);
        LoadDetails details = GetLoad(optionName);
        if (details == null) return;

        int count = Mathf.Min(details.gearSprites.Count, details.gearNames.Count, requiredGearImages.Count, requiredGearNames.Count, requiredGearObjects.Count);

        // Updates the required gear lists and UIs
        for (int i = 0; i < count; i++)
        {
            requiredGearObjects[i].SetActive(true);
            requiredGearImages[i].sprite = details.gearSprites[i];
            requiredGearImages[i].preserveAspect = true;
            requiredGearNames[i].text = details.gearNames[i];
            gearsToCheck.Add(details.gearNames[i]);
        }
    }

    /// <summary>
    /// Coroutine that waits for a short duration after all required gears are placed, enable and disable items to allow the player to move on to the next station.
    /// </summary>
    IEnumerator WaitBeforeMoving()
    {
        // Spawn the load they have selected below the crane
        LoadDetails details = GetLoad(currentSelectedOption);
        if (details == null) yield break;
        if (loadSpawnPos != null)
        {
            Instantiate(details.loadPrefab, loadSpawnPos);
        }
        else
        {
            Debug.LogWarning("Load spawn position (loadSpawnPos) is not assigned.", this);
        }

        yield return new WaitForSeconds(1f);

        if (disables != null && disables.Count != 0)
        {
            foreach (GameObject obj in disables)
            {
                obj.SetActive(false);
            }
        }
        if (enables != null && enables.Count != 0)
        {
            foreach (GameObject obj in enables)
            {
                obj.SetActive(true);
            }
        }
        HideUnusedGears();
        foreach(var gear in placedGears){
            gear.GetComponent<GearSettings>().SwapAttachmentPoint();
        }

        // Provide a navigation arrow to help players recognise where they need to go next
        if (navigation != null)
        {
            GameObject instructionCanvas = GameObject.Find("InstructionCanvas");
            if (instructionCanvas != null)
            {
                Vector3 targetPosition = instructionCanvas.transform.position;

                navigation.positionCount = 2;
                navigation.SetPosition(0, new Vector3(targetPosition.x-1, targetPosition.y+1, targetPosition.z));
                navigation.SetPosition(1, new Vector3(targetPosition.x-1, targetPosition.y, targetPosition.z));
            }
            else
            {
                Debug.LogWarning("InstructionCanvas not found in the scene.");
            }
        UI.Instance.UpdateFeedbackText("Section complete. Please proceed to crane operations.");
        }

        // Reset each required gears to allow them to interact with it and use it in the next station
        foreach (XRGrabInteractable grab in placedGears)
        {
            grab.interactionLayers &= ~InteractionLayerMask.GetMask("LockedGear");
            grab.interactionLayers |= InteractionLayerMask.GetMask("Gear");
        }
    }

    /// <summary>
    /// Checks the gear the player is trying to add against the correct gears.
    /// If the gear is correct, add it a socket and lock its interaction.
    /// In another function, it also prevents the player from reseleecting the load if a gear is already correct and is placed in a socket.
    /// </summary>
    /// <param name="gearName"></param>
    /// <param name="gearInterctable"></param>
    public void AddGear(string gearName, XRGrabInteractable gearInteractable)
    {
        if (string.IsNullOrEmpty(currentSelectedOption))
        {
            // Prevent feedback spam
            if (rejectedGear == gearInteractable) return;
            UI.Instance.UpdateFeedbackText("Select a load first.");
            rejectedGear = gearInteractable;
            return;
        }
        if (!gearsToCheck.Contains(gearName))
        {
            // Prevent feedback spam
            if (rejectedGear == gearInteractable) return;
            rejectedGear = gearInteractable;
            Events.Instance.ItemWrong();
            return;
        }
        if (placedGears.Contains(gearInteractable)) return; // Check if the same item is already placed. If it is, exit the check

        var item = gearInteractable.GetComponent<GearSettings>();
        LoadDetails details = GetLoad(currentSelectedOption);

        // find a free socket
        foreach (XRSocketInteractor socket in gearSockets)
        {
            if (socket.hasSelection) continue;

            if (gearInteractable.isSelected) return;
            socket.socketActive = true;
            socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)gearInteractable);

            placedGears.Add(gearInteractable);
            gearInteractable.interactionLayers &= ~InteractionLayerMask.GetMask("Gear");
            gearInteractable.interactionLayers |= InteractionLayerMask.GetMask("LockedGear");

            if (item != null && item.gearType == GearType.Shackle)
                loadHookSettings.shackle = gearInteractable;
            numberOfGearsFound++;
            Events.Instance.ItemCorrect();
            if (waitingCoroutine != null) return;

            // Check if the player had placed all the gears
            if (gearsToCheck.Count > 0 && gearsToCheck.Count == numberOfGearsFound)
                waitingCoroutine = StartCoroutine(WaitBeforeMoving());
            break;
        }
    }

    void HideUnusedGears()
    {
        foreach (var gear in allGears)
            if (gear != null && !placedGears.Contains(gear))
                gear.gameObject.SetActive(false);
    }
}