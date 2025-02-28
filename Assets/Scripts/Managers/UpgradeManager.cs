using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem instance;
    
    [Header("Upgrades")]
    [SerializeField] private List<Upgrade> availableUpgrades = new List<Upgrade>();
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Transform upgradeButtonContainer;
    
    [Header("Beer Selection")]
    [SerializeField] private GameObject beerButtonPrefab;
    [SerializeField] private Transform beerButtonContainer;
    
    [Header("UI Elements")]
    [SerializeField] private TMP_Text currentMoneyText;
    
    private List<GameObject> upgradeButtons = new List<GameObject>();
    private List<GameObject> beerButtons = new List<GameObject>();
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        //StartCoroutine(InitializeWithDelay());

        // Initialize upgrades
        InitializeUpgrades();

        // Populate the UI elements
        PopulateUpgradeButtons();
        PopulateBeerButtons();

        // Update UI with current money
        if (CurrencyManager.instance != null)
        {
            UpdateMoneyText(CurrencyManager.instance.GetCurrentMoney());

            // Subscribe to currency changes
            CurrencyManager.instance.OnMoneyChanged += UpdateMoneyText;
        }
        else
        {
            Debug.LogWarning("CurrencyManager instance is null in UpgradeSystem Start method");
        }
    }
    private void InitializeUpgrades()
    {
        // Clear any existing upgrades
        availableUpgrades.Clear();

        // POURING UPGRADES
        availableUpgrades.Add(new Upgrade
        {
            upgradeName = "Steady Hand",
            description = "Increases pouring speed for faster filling",
            upgradeType = UpgradeType.PourSpeed,
            baseCost = 50,
            costIncreasePerLevel = 100,
            currentLevel = 0,
            maxLevel = 5,
            baseValue = 6f,  // Initial pour speed value
            valueIncreasePerLevel = 0.5f  // Pour speed increase per level
        });

        // VALUE MULTIPLIER UPGRADES
        availableUpgrades.Add(new Upgrade
        {
            upgradeName = "Premium Glassware",
            description = "Increases the value of all pours by 10% per level",
            upgradeType = UpgradeType.GlassValueMultiplier,
            baseCost = 200,
            costIncreasePerLevel = 250,
            currentLevel = 0,
            maxLevel = 5,
            baseValue = 1f,  // Base multiplier (100%)
            valueIncreasePerLevel = 0.1f  // 10% increase per level
        });

        // ACCURACY UPGRADES
        availableUpgrades.Add(new Upgrade
        {
            upgradeName = "Pouring Precision",
            description = "Makes it easier to hit 'Good' and 'Perfect' ranges",
            upgradeType = UpgradeType.AccuracyBonus,
            baseCost = 100,
            costIncreasePerLevel = 200,
            currentLevel = 0,
            maxLevel = 5,
            baseValue = 0f,
            valueIncreasePerLevel = 0.025f  // Increases the size of good/perfect ranges
        });

        // COMBO UPGRADES
        availableUpgrades.Add(new Upgrade
        {
            upgradeName = "Combo Master",
            description = "Increases the bonus for combos by 10% per level",
            upgradeType = UpgradeType.ComboMultiplier,
            baseCost = 150,
            costIncreasePerLevel = 300,
            currentLevel = 0,
            maxLevel = 3,
            baseValue = 1f,
            valueIncreasePerLevel = 0.1f
        });

        // Load saved upgrade levels
        LoadUpgradeStatus();

        // Apply all upgrades at their current levels
        foreach (var upgrade in availableUpgrades)
        {
            ApplyUpgradeEffect(upgrade);
        }

        Debug.Log($"Initialized {availableUpgrades.Count} upgrades");
    }

    private IEnumerator InitializeWithDelay()
    {
        // Try up to 3 times to find CurrencyManager
        int attempts = 0;
        while (CurrencyManager.instance == null && attempts < 3)
        {
            Debug.Log("Waiting for CurrencyManager to initialize... (attempt " + attempts + ")");
            yield return new WaitForSeconds(0.5f);
            attempts++;
        }

        if (CurrencyManager.instance == null)
        {
            Debug.LogError("Failed to find CurrencyManager after multiple attempts. Make sure it exists in the scene.");
        }
        else
        {
            // Continue with initialization
            LoadUpgradeStatus();
            PopulateUpgradeButtons();
            PopulateBeerButtons();

            // Subscribe to currency changes
            CurrencyManager.instance.OnMoneyChanged += UpdateMoneyText;
        }
    }
    private void UpdateMoneyText(int currentMoney)
    {
        if (currentMoneyText != null)
            currentMoneyText.text = "$" + currentMoney.ToString();
    }

    // Update the RefreshUpgradeUI method to check for mismatches
    public void RefreshUpgradeUI()
    {
        Debug.Log("Refreshing upgrade UI");

        if (CurrencyManager.instance == null)
        {
            Debug.LogWarning("CurrencyManager.instance is null when refreshing upgrade UI");
            return;
        }

        // Update money display
        UpdateMoneyText(CurrencyManager.instance.GetCurrentMoney());

        // Check for button count mismatch that would indicate a problem
        if (upgradeButtons.Count != availableUpgrades.Count)
        {
            Debug.LogWarning("Button count mismatch detected: " + upgradeButtons.Count +
                             " buttons vs " + availableUpgrades.Count + " upgrades. Re-populating buttons.");
            PopulateUpgradeButtons();
            return;
        }

        // Update existing button states
        UpdateUpgradeButtonStates();
    }



    public void RefreshBeerUI()
    {
        // Check if CurrencyManager exists
        if (CurrencyManager.instance == null)
        {
            Debug.LogError("CurrencyManager is null when trying to refresh beer UI");
            return;
        }

        // Update money display
        UpdateMoneyText(CurrencyManager.instance.GetCurrentMoney());

        // Always re-populate buttons to ensure they match the current state
        PopulateBeerButtons();
    }
    // Add this method to UpgradeSystem.cs
    public void ForceRefreshAllUI()
    {
        Debug.Log("Force refreshing all UI");

        // If CurrencyManager exists, get the current money
        int currentMoney = 0;
        if (CurrencyManager.instance != null)
        {
            currentMoney = CurrencyManager.instance.GetCurrentMoney();
            Debug.Log("Current money: $" + currentMoney);
        }
        else
        {
            Debug.LogWarning("CurrencyManager.instance is null during UI refresh!");
        }

        // Update money display
        UpdateMoneyText(currentMoney);

        // Re-populate all buttons
        PopulateUpgradeButtons();
        PopulateBeerButtons();

        Debug.Log("UI refresh complete");
    }

    private void PopulateUpgradeButtons()
    {
        Debug.Log("Populating upgrade buttons");

        // Clear existing buttons
        foreach (GameObject button in upgradeButtons)
        {
            if (button != null)
                Destroy(button);
        }
        upgradeButtons.Clear();

        // Check if prefab and container exist
        if (upgradeButtonPrefab == null)
        {
            Debug.LogError("Upgrade button prefab is not assigned!");
            return;
        }

        if (upgradeButtonContainer == null)
        {
            Debug.LogError("Upgrade button container is not assigned!");
            return;
        }

        // Create new buttons for each upgrade
        foreach (Upgrade upgrade in availableUpgrades)
        {
            if (upgrade == null) continue;

            // Create button instance
            GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
            upgradeButtons.Add(buttonObj);

            Debug.Log("Created upgrade button for: " + upgrade.upgradeName);

            // Find text components using direct paths based on hierarchy
            Transform upgradeNameTransform = buttonObj.transform.Find("UpgradeName");
            Transform descriptionTransform = buttonObj.transform.Find("Description");
            Transform unlockPanelTransform = buttonObj.transform.Find("UnlockPanel");

            if (unlockPanelTransform == null)
            {
                Debug.LogError("UnlockPanel not found in button hierarchy for " + upgrade.upgradeName);
                continue;
            }

            Transform unlockButtonTransform = unlockPanelTransform.Find("UnlockButton");

            if (unlockButtonTransform == null)
            {
                Debug.LogError("UnlockButton not found in button hierarchy for " + upgrade.upgradeName);
                continue;
            }

            Transform levelTransform = unlockButtonTransform.Find("Level");
            Transform costTransform = unlockButtonTransform.Find("Cost");

            // Set text values if components exist
            if (upgradeNameTransform != null && upgradeNameTransform.GetComponent<TMP_Text>() != null)
            {
                upgradeNameTransform.GetComponent<TMP_Text>().text = upgrade.upgradeName;
                Debug.Log("Set upgrade name: " + upgrade.upgradeName);
            }
            else
            {
                Debug.LogWarning("UpgradeName component not found for " + upgrade.upgradeName);
            }

            if (descriptionTransform != null && descriptionTransform.GetComponent<TMP_Text>() != null)
            {
                descriptionTransform.GetComponent<TMP_Text>().text = upgrade.description;
                Debug.Log("Set description for " + upgrade.upgradeName);
            }
            else
            {
                Debug.LogWarning("Description component not found for " + upgrade.upgradeName);
            }

            if (levelTransform != null && levelTransform.GetComponent<TMP_Text>() != null)
            {
                levelTransform.GetComponent<TMP_Text>().text = "Level: " + upgrade.currentLevel + "/" + upgrade.maxLevel;
                Debug.Log("Set level text for " + upgrade.upgradeName);
            }
            else
            {
                Debug.LogWarning("Level component not found for " + upgrade.upgradeName);
            }

            if (costTransform != null && costTransform.GetComponent<TMP_Text>() != null)
            {
                if (upgrade.currentLevel >= upgrade.maxLevel)
                {
                    costTransform.GetComponent<TMP_Text>().text = "MAXED OUT";
                }
                else
                {
                    costTransform.GetComponent<TMP_Text>().text = "$" + GetNextUpgradeCost(upgrade);
                }
                Debug.Log("Set cost text for " + upgrade.upgradeName);
            }
            else
            {
                Debug.LogWarning("Cost component not found for " + upgrade.upgradeName);
            }

            // Set up the button's click handler and interactability
            SetupUpgradeButton(buttonObj, upgrade);
        }

        Debug.Log("Finished populating " + upgradeButtons.Count + " upgrade buttons");
    }

    public void PopulateBeerButtons()
    {
        Debug.Log("Populating beer buttons");

        // Clear existing buttons
        foreach (GameObject button in beerButtons)
        {
            if (button != null)
                Destroy(button);
        }
        beerButtons.Clear();

        // Check if CurrencyManager exists
        if (CurrencyManager.instance == null)
        {
            Debug.LogError("CurrencyManager instance is null. Cannot populate beer buttons.");
            return;
        }

        // Get beer types
        BeerType[] beerTypes = CurrencyManager.instance.GetAvailableBeerTypes();

        Debug.Log("Found " + (beerTypes?.Length ?? 0) + " beer types");

        if (beerTypes == null || beerTypes.Length == 0)
        {
            Debug.LogWarning("No beer types available in CurrencyManager.");
            return;
        }

        // Check if prefab and container exist
        if (beerButtonPrefab == null)
        {
            Debug.LogError("Beer button prefab is not assigned!");
            return;
        }

        if (beerButtonContainer == null)
        {
            Debug.LogError("Beer button container is not assigned!");
            return;
        }

        // Create buttons for each beer type
        foreach (BeerType beerType in beerTypes)
        {
            if (beerType == null) continue;

            // Create button instance
            GameObject buttonObj = Instantiate(beerButtonPrefab, beerButtonContainer);
            if (buttonObj == null) continue;

            beerButtons.Add(buttonObj);

            // Find text component for beer name - use specific path
            Transform beerNameTransform = buttonObj.transform.Find("BeerName");
            if (beerNameTransform != null)
            {
                TMP_Text beerNameText = beerNameTransform.GetComponent<TMP_Text>();
                if (beerNameText != null)
                {
                    beerNameText.text = beerType.beerName;
                    Debug.Log("Set beer name: " + beerType.beerName);
                }
            }
            else
            {
                Debug.LogWarning("BeerName transform not found on beer button");
            }

            // Get references to important parts of the button
            Transform unlockPanelTransform = buttonObj.transform.Find("UnlockPanel");
            Transform selectButtonTransform = buttonObj.transform.Find("SelectButton");
            Transform selectedTransform = buttonObj.transform.Find("Selected");

            // Setup button appearance based on unlock status
            if (beerType.isUnlocked)
            {
                // This beer is unlocked - hide unlock panel, show select/selected
                if (unlockPanelTransform != null)
                    unlockPanelTransform.gameObject.SetActive(false);

                if (selectButtonTransform != null && selectedTransform != null)
                {
                    Button selectButton = selectButtonTransform.GetComponent<Button>();

                    // Check if this is the currently selected beer
                    bool isSelected = false;
                    if (CurrencyManager.instance.GetCurrentBeerType() != null)
                    {
                        isSelected = CurrencyManager.instance.GetCurrentBeerType().beerName == beerType.beerName;
                    }

                    // Configure button
                    if (isSelected)
                    {
                        // Show "Selected" text instead of button
                        selectedTransform.gameObject.SetActive(true);
                        selectButtonTransform.gameObject.SetActive(false);
                    }
                    else
                    {
                        // Set up select button
                        selectedTransform.gameObject.SetActive(false);
                        selectButtonTransform.gameObject.SetActive(true);

                        if (selectButton != null)
                        {
                            // Clear existing listeners
                            selectButton.onClick.RemoveAllListeners();
                            // Add new listener
                            selectButton.onClick.AddListener(() => SelectBeerType(beerType));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("SelectButton or Selected transform not found");
                }
            }
            else
            {
                // This beer is locked - show unlock panel, hide select/selected
                if (unlockPanelTransform != null)
                {
                    unlockPanelTransform.gameObject.SetActive(true);

                    // Set up unlock button
                    Transform unlockButtonTransform = unlockPanelTransform.Find("UnlockButton");
                    if (unlockButtonTransform != null)
                    {
                        Button unlockButton = unlockButtonTransform.GetComponent<Button>();
                        if (unlockButton != null)
                        {
                            // Clear existing listeners
                            unlockButton.onClick.RemoveAllListeners();
                            // Add new listener
                            unlockButton.onClick.AddListener(() => UnlockBeerType(beerType));

                            // Find and update the unlock cost text
                            int cost = beerType.unlockCost;
                            string costText = "$" + cost;
                            TMP_Text valueText = unlockButtonTransform.Find("Value")?.GetComponent<TMP_Text>();
                            if (valueText != null)
                            {
                                valueText.text = "$" + beerType.unlockCost;
                            }

                            // Disable button if not enough money
                            unlockButton.interactable = CurrencyManager.instance.GetCurrentMoney() >= beerType.unlockCost;
                        }
                    }
                }

                // Hide select/selected elements
                if (selectButtonTransform != null)
                    selectButtonTransform.gameObject.SetActive(false);

                if (selectedTransform != null)
                    selectedTransform.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateUpgradeButtonStates()
    {
        if (availableUpgrades == null || availableUpgrades.Count == 0)
        {
            Debug.LogWarning("No available upgrades to update");
            return;
        }

        if (upgradeButtons == null || upgradeButtons.Count == 0)
        {
            Debug.LogWarning("No upgrade buttons to update");
            return;
        }

        if (CurrencyManager.instance == null)
        {
            Debug.LogWarning("CurrencyManager instance is null when updating button states");
            return;
        }

        // Log debug info
        Debug.Log("Updating " + upgradeButtons.Count + " button states for " + availableUpgrades.Count + " upgrades");
        int currentMoney = CurrencyManager.instance.GetCurrentMoney();

        if (upgradeButtons.Count != availableUpgrades.Count)
        {
            Debug.LogWarning("Button count mismatch: " + upgradeButtons.Count + " buttons vs " +
                             availableUpgrades.Count + " upgrades.");
        }

        for (int i = 0; i < availableUpgrades.Count && i < upgradeButtons.Count; i++)
        {
            Upgrade upgrade = availableUpgrades[i];
            GameObject buttonObj = upgradeButtons[i];

            if (upgrade == null || buttonObj == null) continue;

            // Find text components based on the hierarchy
            Transform unlockPanel = buttonObj.transform.Find("UnlockPanel");
            if (unlockPanel == null) continue;

            Transform unlockButton = unlockPanel.Find("UnlockButton");
            if (unlockButton == null) continue;

            Transform levelTransform = unlockButton.Find("Level");
            Transform costTransform = unlockButton.Find("Cost");

            // Update level text
            if (levelTransform != null)
            {
                TMP_Text levelText = levelTransform.GetComponent<TMP_Text>();
                if (levelText != null)
                {
                    string newText = "Level: " + upgrade.currentLevel + "/" + upgrade.maxLevel;
                    if (levelText.text != newText) // Only update if changed
                    {
                        levelText.text = newText;
                        Debug.Log("Updated level text for " + upgrade.upgradeName + " to: " + levelText.text);
                    }
                }
            }

            // Update cost text
            if (costTransform != null)
            {
                TMP_Text costText = costTransform.GetComponent<TMP_Text>();
                if (costText != null)
                {
                    string newText;
                    if (upgrade.currentLevel >= upgrade.maxLevel)
                    {
                        newText = "MAXED OUT";
                    }
                    else
                    {
                        int cost = GetNextUpgradeCost(upgrade);
                        newText = "$" + cost;
                    }

                    if (costText.text != newText) // Only update if changed
                    {
                        costText.text = newText;
                        Debug.Log("Updated cost text for " + upgrade.upgradeName + " to: " + costText.text);
                    }
                }
            }

            // Update button interactability
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int cost = GetNextUpgradeCost(upgrade);
                bool canAfford = currentMoney >= cost;
                bool notMaxed = upgrade.currentLevel < upgrade.maxLevel;

                bool shouldBeInteractable = canAfford && notMaxed;
                if (button.interactable != shouldBeInteractable) // Only update if changed
                {
                    button.interactable = shouldBeInteractable;
                    Debug.Log("Updated button interactability for " + upgrade.upgradeName + " to: " + button.interactable);
                }
            }
        }
    }

    private void UpdateBeerButtonStates()
    {
        if (CurrencyManager.instance == null) return;
        if (beerButtons == null || beerButtons.Count == 0) return;

        BeerType[] availableBeerTypes = CurrencyManager.instance.GetAvailableBeerTypes();
        if (availableBeerTypes == null || availableBeerTypes.Length == 0) return;

        foreach (GameObject buttonObj in beerButtons)
        {
            if (buttonObj == null) continue;

            Transform beerNameTransform = buttonObj.transform.Find("BeerName");
            if (beerNameTransform == null) continue;

            TMP_Text beerNameText = beerNameTransform.GetComponent<TMP_Text>();
            if (beerNameText == null) continue;

            string beerName = beerNameText.text;

            // Find this beer type
            BeerType foundBeer = null;
            foreach (BeerType bt in availableBeerTypes)
            {
                if (bt != null && bt.beerName == beerName)
                {
                    foundBeer = bt;
                    break;
                }
            }

            if (foundBeer != null)
            {
                Transform selectedTransform = buttonObj.transform.Find("Selected");
                Transform selectButtonTransform = buttonObj.transform.Find("SelectButton");
                Transform unlockPanelTransform = buttonObj.transform.Find("UnlockPanel");

                if (selectedTransform == null || selectButtonTransform == null || unlockPanelTransform == null)
                    continue;

                if (foundBeer.isUnlocked)
                {
                    // Update selected status
                    BeerType currentBeer = CurrencyManager.instance.GetCurrentBeerType();
                    if (currentBeer != null && foundBeer.beerName == currentBeer.beerName)
                    {
                        selectedTransform.gameObject.SetActive(true);
                        selectButtonTransform.gameObject.SetActive(false);
                    }
                    else
                    {
                        selectedTransform.gameObject.SetActive(false);
                        selectButtonTransform.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Update unlock button state based on money
                    Button unlockButton = unlockPanelTransform.Find("UnlockButton")?.GetComponent<Button>();
                    if (unlockButton != null)
                    {
                        unlockButton.interactable = CurrencyManager.instance.GetCurrentMoney() >= foundBeer.unlockCost;
                    }
                }
            }
        }
    }

    public void PurchaseUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogError("Attempt to purchase null upgrade!");
            return;
        }

        Debug.Log("PurchaseUpgrade called for: " + upgrade.upgradeName);
        int cost = GetNextUpgradeCost(upgrade);

        // Verify CurrencyManager exists
        if (CurrencyManager.instance == null)
        {
            Debug.LogError("CurrencyManager.instance is null in PurchaseUpgrade!");
            return;
        }

        int currentMoney = CurrencyManager.instance.GetCurrentMoney();
        Debug.Log("Attempting purchase - Cost: $" + cost + ", Current money: $" + currentMoney +
                  ", Current level: " + upgrade.currentLevel + "/" + upgrade.maxLevel);

        // Check if can afford it and are not at max level
        bool canAfford = currentMoney >= cost;
        bool notMaxLevel = upgrade.currentLevel < upgrade.maxLevel;

        if (!canAfford)
        {
            Debug.LogWarning("Cannot afford upgrade " + upgrade.upgradeName + " - Cost: $" + cost +
                             ", Current money: $" + currentMoney);
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("wrong");
            return;
        }

        if (!notMaxLevel)
        {
            Debug.LogWarning("Upgrade " + upgrade.upgradeName + " already at max level: " +
                             upgrade.currentLevel + "/" + upgrade.maxLevel);
            return;
        }

        // Try to spend the money
        bool spendSuccess = CurrencyManager.instance.SpendMoney(cost);

        if (spendSuccess)
        {
            Debug.Log("Upgrade purchase successful for " + upgrade.upgradeName);

            // Increment level
            upgrade.currentLevel++;

            // Save the upgrade status
            SaveUpgradeStatus();

            // Apply upgrade effect
            ApplyUpgradeEffect(upgrade);

            // Immediately update UI for this specific button
            UpdateSpecificButtonUI(upgrade);

            // Also update all buttons to refresh interactability based on new money amount
            UpdateUpgradeButtonStates();

            // Play success sound
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("upgrade");

            // Show money decrease (optional)
            if (GameManager.instance != null)
            {
                GameManager.instance.ShowMoneyEarned(-cost);
            }
        }
        else
        {
            Debug.LogError("Failed to spend money for upgrade");
        }
    }
    // New method to update a specific button immediately after purchase
    private void UpdateSpecificButtonUI(Upgrade upgrade)
    {
        // Find the button associated with this upgrade
        int upgradeIndex = availableUpgrades.IndexOf(upgrade);
        if (upgradeIndex < 0 || upgradeIndex >= upgradeButtons.Count)
        {
            Debug.LogWarning("Cannot find button for upgrade: " + upgrade.upgradeName);
            return;
        }

        GameObject buttonObj = upgradeButtons[upgradeIndex];
        if (buttonObj == null)
        {
            Debug.LogWarning("Button object is null for upgrade: " + upgrade.upgradeName);
            return;
        }

        Debug.Log("Immediately updating UI for " + upgrade.upgradeName + " button");

        // Get references to text components
        Transform unlockPanel = buttonObj.transform.Find("UnlockPanel");
        if (unlockPanel == null) return;

        Transform unlockButton = unlockPanel.Find("UnlockButton");
        if (unlockButton == null) return;

        // Update level text
        Transform levelTransform = unlockButton.Find("Level");
        if (levelTransform != null)
        {
            TMP_Text levelText = levelTransform.GetComponent<TMP_Text>();
            if (levelText != null)
            {
                levelText.text = "Level: " + upgrade.currentLevel + "/" + upgrade.maxLevel;
                Debug.Log("Updated level text to: " + levelText.text);
            }
        }

        // Update cost text
        Transform costTransform = unlockButton.Find("Cost");
        if (costTransform != null)
        {
            TMP_Text costText = costTransform.GetComponent<TMP_Text>();
            if (costText != null)
            {
                if (upgrade.currentLevel >= upgrade.maxLevel)
                {
                    costText.text = "MAX";
                }
                else
                {
                    int newCost = GetNextUpgradeCost(upgrade);
                    costText.text = "$" + newCost;
                }
                Debug.Log("Updated cost text to: " + costText.text);
            }
        }

        // Update button interactability
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            int newCost = GetNextUpgradeCost(upgrade);
            int currentMoney = CurrencyManager.instance.GetCurrentMoney();
            bool canAfford = currentMoney >= newCost;
            bool notMaxed = upgrade.currentLevel < upgrade.maxLevel;

            button.interactable = canAfford && notMaxed;
            Debug.Log("Updated button interactability to: " + button.interactable);
        }
    }

    // Enhanced button creation method with improved onClick handling
    private void SetupUpgradeButton(GameObject buttonObj, Upgrade upgrade)
    {
        if (buttonObj == null || upgrade == null) return;

        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Button component missing on upgrade button object!");
            return;
        }

        // First, remove any existing listeners to prevent duplicates
        button.onClick.RemoveAllListeners();

        // Store a copy of the upgrade to use in the callback
        Upgrade upgradeCopy = upgrade;

        // Add the click listener with debug logging
        button.onClick.AddListener(() => {
            Debug.Log("Button clicked for upgrade: " + upgradeCopy.upgradeName);
            PurchaseUpgrade(upgradeCopy);
        });

        // Check interactability conditions
        bool canAfford = false;
        bool notMaxLevel = upgrade.currentLevel < upgrade.maxLevel;

        if (CurrencyManager.instance != null)
        {
            int cost = GetNextUpgradeCost(upgrade);
            int currentMoney = CurrencyManager.instance.GetCurrentMoney();
            canAfford = currentMoney >= cost;

            Debug.Log("Button for " + upgrade.upgradeName + " - Cost: $" + cost +
                     ", Current money: $" + currentMoney + ", Can afford: " + canAfford);
        }
        else
        {
            Debug.LogWarning("CurrencyManager.instance is null when setting up button!");
        }

        // Set interactability
        button.interactable = notMaxLevel && canAfford;

        Debug.Log("Button for " + upgrade.upgradeName + " set to interactable: " + button.interactable);
    }

    public void UnlockBeerType(BeerType beerType)
    {
        // Add a debug statement for troubleshooting
        Debug.Log("Attempting to unlock beer: " + beerType.beerName + " (Cost: $" + beerType.unlockCost + ")");

        // Check if player has enough money
        if (CurrencyManager.instance != null && CurrencyManager.instance.SpendMoney(beerType.unlockCost))
        {
            Debug.Log("Successfully unlocked beer: " + beerType.beerName);

            // Unlock the beer
            beerType.isUnlocked = true;

            // Show a money deduction message
            if (GameManager.instance != null)
            {
                GameManager.instance.ShowMoneyEarned(-beerType.unlockCost);
            }

            // Refresh beer buttons
            PopulateBeerButtons();

            SelectBeerType(beerType);
        }
        else
        {
            Debug.Log("Not enough money to unlock beer: " + beerType.beerName);

        }
    }

    public void SelectBeerType(BeerType beerType)
    {
        Debug.Log("Selecting beer: " + beerType.beerName);

        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.SetBeerType(beerType);

            // Update the liquid color in game
            if (GameManager.instance != null && GameManager.instance.currentJar != null)
            {
                Debug.Log("Updating current jar liquid color");
                UpdateLiquidColor(beerType.beerColor);
            }

            // Play selection sound
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("select");

            // Force refresh the entire beer button list
            PopulateBeerButtons();
        }
    }

    private int GetNextUpgradeCost(Upgrade upgrade)
    {
        // Cost increases with each level
        return upgrade.baseCost + (upgrade.costIncreasePerLevel * upgrade.currentLevel);
    }
    
    private void ApplyUpgradeEffect(Upgrade upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.PourSpeed:
                // This affects the base rise amount - the random factor is applied on top of this
                float baseRiseValue = upgrade.baseValue + (upgrade.valueIncreasePerLevel * upgrade.currentLevel);
                // Store this value in PlayerPrefs so it can be accessed by GameManager
                PlayerPrefs.SetFloat("PourSpeedBase", baseRiseValue);
                break;
                
            case UpgradeType.GlassValueMultiplier:
                // Increases points for all jars
                PlayerPrefs.SetFloat("JarValueMultiplier", 1 + (upgrade.valueIncreasePerLevel * upgrade.currentLevel));
                break;
                
            case UpgradeType.AccuracyBonus:
                // Make the "good" and "perfect" ranges larger
                PlayerPrefs.SetFloat("GoodRangeBonus", upgrade.valueIncreasePerLevel * upgrade.currentLevel);
                PlayerPrefs.SetFloat("PerfectRangeBonus", (upgrade.valueIncreasePerLevel/2) * upgrade.currentLevel);
                break;
                
            case UpgradeType.ComboMultiplier:
                // Increase combo multiplier effect
                PlayerPrefs.SetFloat("ComboMultiplier", 1 + (upgrade.valueIncreasePerLevel * upgrade.currentLevel));
                break;
                
        }
        
        // Save changes
        PlayerPrefs.Save();
    }

    private void UpdateLiquidColor(Color newColor)
    {
        SpriteRenderer liquid = GameManager.instance.currentJar.myLiquid.spriteRen;
        liquid.color = newColor;

    }

    private void SaveUpgradeStatus()
    {
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            PlayerPrefs.SetInt("Upgrade_" + i + "_Level", availableUpgrades[i].currentLevel);
        }
        PlayerPrefs.Save();
    }
    
    private void LoadUpgradeStatus()
    {
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            availableUpgrades[i].currentLevel = PlayerPrefs.GetInt("Upgrade_" + i + "_Level", 0);
            
            // Apply loaded upgrades
            ApplyUpgradeEffect(availableUpgrades[i]);
        }
    }
    
    private void SaveBeerUnlockStatus()
    {
        BeerType[] beerTypes = CurrencyManager.instance.GetAvailableBeerTypes();
        for (int i = 0; i < beerTypes.Length; i++)
        {
            PlayerPrefs.SetInt("Beer_" + i + "_Unlocked", beerTypes[i].isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.OnMoneyChanged -= UpdateMoneyText;
    }
}

[System.Serializable]
public enum UpgradeType
{
    PourSpeed,
    GlassValueMultiplier,
    AccuracyBonus,
    ComboMultiplier,
    ExtraLife
}

[System.Serializable]
public class Upgrade
{
    public string upgradeName;
    public string description;
    public UpgradeType upgradeType;
    public int baseCost;
    public int costIncreasePerLevel;
    public int currentLevel;
    public int maxLevel;
    public float baseValue;
    public float valueIncreasePerLevel;

    // Override Equals and GetHashCode for proper equality comparison
    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Upgrade))
            return false;

        Upgrade other = (Upgrade)obj;
        return this.upgradeType == other.upgradeType &&
               this.upgradeName == other.upgradeName;
    }

    public override int GetHashCode()
    {
        // Create a unique hash code based on name and type
        return (upgradeName + upgradeType.ToString()).GetHashCode();
    }
}
