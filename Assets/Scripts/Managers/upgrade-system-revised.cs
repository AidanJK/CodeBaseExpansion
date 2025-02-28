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
        StartCoroutine(InitializeWithDelay());
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
    
    public void RefreshUpgradeUI()
    {
        UpdateMoneyText(CurrencyManager.instance.GetCurrentMoney());
        UpdateUpgradeButtonStates();
    }
    
    public void RefreshBeerUI()
    {
        UpdateMoneyText(CurrencyManager.instance.GetCurrentMoney());
        UpdateBeerButtonStates();
    }
    
    private void PopulateUpgradeButtons()
    {
        // Clear existing buttons
        foreach (GameObject button in upgradeButtons)
        {
            Destroy(button);
        }
        upgradeButtons.Clear();
        
        // Create new buttons for each upgrade
        foreach (Upgrade upgrade in availableUpgrades)
        {
            GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
            upgradeButtons.Add(buttonObj);
            
            // Set button text and data
            buttonObj.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = upgrade.upgradeName;
            buttonObj.transform.Find("Description").GetComponent<TMP_Text>().text = upgrade.description;
            buttonObj.transform.Find("Cost").GetComponent<TMP_Text>().text = "$" + GetNextUpgradeCost(upgrade);
            buttonObj.transform.Find("Level").GetComponent<TMP_Text>().text = "Level: " + upgrade.currentLevel + "/" + upgrade.maxLevel;
            
            // Add button click listener
            Button button = buttonObj.GetComponent<Button>();
            Upgrade upgradeRef = upgrade; // Create local variable for closure
            button.onClick.AddListener(() => PurchaseUpgrade(upgradeRef));
        }
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

            // Find text component for beer name
            TMP_Text nameText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = beerType.beerName;
            }

            // Setup button appearance based on unlock status
            if (beerType.isUnlocked)
            {
                // Find select button
                Button selectButton = buttonObj.GetComponentInChildren<Button>();
                if (selectButton != null)
                {
                    // Check if this is the currently selected beer
                    bool isSelected = CurrencyManager.instance.GetCurrentBeerType() == beerType;

                    // Configure button
                    if (isSelected)
                    {
                        // Show "Selected" text instead of button
                        Transform selectedText = buttonObj.transform.Find("Selected");
                        if (selectedText != null)
                        {
                            selectedText.gameObject.SetActive(true);
                            selectButton.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        // Set up select button
                        selectButton.onClick.AddListener(() => SelectBeerType(beerType));

                        Transform selectedText = buttonObj.transform.Find("Selected");
                        if (selectedText != null)
                        {
                            selectedText.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                // Set up unlock button
                Button unlockButton = buttonObj.GetComponentInChildren<Button>();
                if (unlockButton != null)
                {
                    unlockButton.onClick.AddListener(() => UnlockBeerType(beerType));
                    unlockButton.GetComponentInChildren<TMP_Text>().text = "Unlock: $" + beerType.unlockCost;

                    // Disable if not enough money
                    unlockButton.interactable = CurrencyManager.instance.GetCurrentMoney() >= beerType.unlockCost;
                }

                // Show cost information
                TMP_Text valueText = buttonObj.transform.Find("Value")?.GetComponent<TMP_Text>();
                if (valueText != null)
                {
                    valueText.text = "Value: $" + beerType.baseValue + " per perfect pour";
                }
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
        if (CurrencyManager.instance.SpendMoney(beerType.unlockCost))
        {
            // Unlock the beer
            beerType.isUnlocked = true;
            
            // Save unlock status
            SaveBeerUnlockStatus();
            
            // Play unlock sound
            AudioManager.instance.PlaySound("upgrade");
            
            // Refresh beer buttons
            PopulateBeerButtons();
        }
    }

    public void SelectBeerType(BeerType beerType)
    {
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.SetBeerType(beerType);

            // Update the liquid color in game
            UpdateLiquidColor(beerType.beerColor);

            // Play selection sound
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
                
            case UpgradeType.ExtraLife:
                // Add an extra life (simply increase max lives)
                PlayerPrefs.SetInt("MaxLives", 3 + upgrade.currentLevel);
                break;
        }
        
        // Save changes
        PlayerPrefs.Save();
    }
    
    private void UpdateLiquidColor(Color newColor)
    {
        // Find all active jars and update their liquid color
        Jar currentJar = GameManager.instance.currentJar;
        if (currentJar != null && currentJar.myLiquid != null && currentJar.myLiquid.spriteRen != null)
        {
            currentJar.myLiquid.spriteRen.color = newColor;
        }
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
