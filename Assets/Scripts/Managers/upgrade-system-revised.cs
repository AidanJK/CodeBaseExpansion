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
        InitializeUpgrades();
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

    /*private IEnumerator InitializeWithDelay()
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
    }*/
    private void UpdateMoneyText(int currentMoney)
    {
        if (currentMoneyText != null)
            currentMoneyText.text = "$" + currentMoney.ToString();
    }
    
    public void RefreshUpgradeUI()
    {
        UpdateMoneyText(CurrencyManager.instance.GetCurrentMoney());
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

            // Find and set text components (directly by name)
            Transform upgradeNameTransform = buttonObj.transform.Find("UpgradeName");
            Transform descriptionTransform = buttonObj.transform.Find("Description");
            Transform costTransform = buttonObj.transform.Find("Cost");
            Transform levelTransform = buttonObj.transform.Find("Level");

            // Set text values if components exist
            if (upgradeNameTransform != null && upgradeNameTransform.GetComponent<TMPro.TextMeshProUGUI>() != null)
                upgradeNameTransform.GetComponent<TMPro.TextMeshProUGUI>().text = upgrade.upgradeName;

            if (descriptionTransform != null && descriptionTransform.GetComponent<TMPro.TextMeshProUGUI>() != null)
                descriptionTransform.GetComponent<TMPro.TextMeshProUGUI>().text = upgrade.description;

            if (costTransform != null && costTransform.GetComponent<TMPro.TextMeshProUGUI>() != null)
                costTransform.GetComponent<TMPro.TextMeshProUGUI>().text = "$" + GetNextUpgradeCost(upgrade);

            if (levelTransform != null && levelTransform.GetComponent<TMPro.TextMeshProUGUI>() != null)
                levelTransform.GetComponent<TMPro.TextMeshProUGUI>().text = "Level: " + upgrade.currentLevel + "/" + upgrade.maxLevel;

            // Add button click listener
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                // Create a local copy for the closure
                Upgrade upgradeRef = upgrade;

                // Clear existing listeners and add new one
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => PurchaseUpgrade(upgradeRef));

                // Set button interactability based on upgrade state and cost
                int cost = GetNextUpgradeCost(upgrade);
                button.interactable = upgrade.currentLevel < upgrade.maxLevel &&
                                     CurrencyManager.instance.GetCurrentMoney() >= cost;

                if (upgrade.currentLevel >= upgrade.maxLevel && costTransform != null &&
                    costTransform.GetComponent<TMPro.TextMeshProUGUI>() != null)
                {
                    costTransform.GetComponent<TMPro.TextMeshProUGUI>().text = "MAXED OUT";
                }
            }
        }

        Debug.Log("Finished populating " + upgradeButtons.Count + " upgrade buttons");
    }

    private void PopulateBeerButtons()
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

        // Debug log to check if we're getting beer types
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
        if (availableUpgrades == null || availableUpgrades.Count == 0) return;
        if (upgradeButtons == null || upgradeButtons.Count == 0) return;
        if (CurrencyManager.instance == null) return;

        for (int i = 0; i < availableUpgrades.Count && i < upgradeButtons.Count; i++)
        {
            Upgrade upgrade = availableUpgrades[i];
            GameObject buttonObj = upgradeButtons[i];

            if (upgrade == null || buttonObj == null) continue;

            // Update button texts
            Transform levelTransform = buttonObj.transform.Find("Level");
            Transform costTransform = buttonObj.transform.Find("Cost");

            if (levelTransform != null && levelTransform.GetComponent<TMP_Text>() != null)
            {
                levelTransform.GetComponent<TMP_Text>().text = "Level: " + upgrade.currentLevel + "/" + upgrade.maxLevel;
            }

            if (costTransform != null && costTransform.GetComponent<TMP_Text>() != null)
            {
                costTransform.GetComponent<TMP_Text>().text = "$" + GetNextUpgradeCost(upgrade);
            }

            // Disable button if max level or not enough money
            Button button = buttonObj.GetComponent<Button>();
            if (button == null) continue;

            int cost = GetNextUpgradeCost(upgrade);

            if (upgrade.currentLevel >= upgrade.maxLevel)
            {
                button.interactable = false;
                if (costTransform != null && costTransform.GetComponent<TMP_Text>() != null)
                {
                    costTransform.GetComponent<TMP_Text>().text = "MAXED OUT";
                }
            }
            else if (CurrencyManager.instance.GetCurrentMoney() < cost)
            {
                button.interactable = false;
            }
            else
            {
                button.interactable = true;
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
        int cost = GetNextUpgradeCost(upgrade);
        
        if (CurrencyManager.instance.SpendMoney(cost) && upgrade.currentLevel < upgrade.maxLevel)
        {
            upgrade.currentLevel++;
            SaveUpgradeStatus();
            
            // Apply upgrade effect
            ApplyUpgradeEffect(upgrade);
            
            // Update UI
            UpdateUpgradeButtonStates();
            
            // Play sound effect
            AudioManager.instance.PlaySound("upgrade");
        }
    }

    public void UnlockBeerType(BeerType beerType)
    {
        // Add a debug statement for troubleshooting
        Debug.Log("Attempting to unlock beer: " + beerType.beerName + " (Cost: $" + beerType.unlockCost + ")");

        // Check if we have enough money
        if (CurrencyManager.instance != null && CurrencyManager.instance.SpendMoney(beerType.unlockCost))
        {
            Debug.Log("Successfully unlocked beer: " + beerType.beerName);

            // Unlock the beer
            beerType.isUnlocked = true;

            // Play unlock sound
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("upgrade");

            // Show a money deduction message
            if (GameManager.instance != null)
            {
                GameManager.instance.ShowMoneyEarned(-beerType.unlockCost);
            }

            // Refresh beer buttons
            PopulateBeerButtons();

            // Optionally auto-select this beer
            SelectBeerType(beerType);
        }
        else
        {
            Debug.Log("Not enough money to unlock beer: " + beerType.beerName);

            // Optionally play a "can't afford" sound
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("wrong");
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
            // This will regenerate all buttons with correct selected states
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
}
