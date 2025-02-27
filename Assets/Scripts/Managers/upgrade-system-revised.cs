using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

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
        // Load saved upgrades
        LoadUpgradeStatus();
        
        // Generate initial UI elements
        PopulateUpgradeButtons();
        PopulateBeerButtons();
        
        // Subscribe to currency changes
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.OnMoneyChanged += UpdateMoneyText;
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
        // Clear existing buttons
        foreach (GameObject button in beerButtons)
        {
            Destroy(button);
        }
        beerButtons.Clear();
        
        if (CurrencyManager.instance == null) return;
        
        // Create buttons for each beer type
        foreach (BeerType beerType in CurrencyManager.instance.GetAvailableBeerTypes())
        {
            GameObject buttonObj = Instantiate(beerButtonPrefab, beerButtonContainer);
            beerButtons.Add(buttonObj);
            
            // Set button text and image
            buttonObj.transform.Find("BeerName").GetComponent<TMP_Text>().text = beerType.beerName;
            
            if (beerType.isUnlocked)
            {
                // Show selection button for unlocked beers
                buttonObj.transform.Find("UnlockPanel").gameObject.SetActive(false);
                buttonObj.transform.Find("SelectButton").gameObject.SetActive(true);
                
                // Value display
                buttonObj.transform.Find("Value").GetComponent<TMP_Text>().text = "Value: $" + beerType.baseValue + " per perfect pour";
                
                // Set image
                if (buttonObj.transform.Find("BeerImage").GetComponent<Image>() != null && beerType.beerSprite != null)
                    buttonObj.transform.Find("BeerImage").GetComponent<Image>().sprite = beerType.beerSprite;
                
                // Show "selected" for current beer
                BeerType currentBeer = CurrencyManager.instance.GetCurrentBeerType();
                if (beerType.beerName == currentBeer.beerName)
                {
                    buttonObj.transform.Find("Selected").gameObject.SetActive(true);
                    buttonObj.transform.Find("SelectButton").gameObject.SetActive(false);
                }
                else
                {
                    buttonObj.transform.Find("Selected").gameObject.SetActive(false);
                }
                
                // Add select button listener
                Button selectButton = buttonObj.transform.Find("SelectButton").GetComponent<Button>();
                BeerType beerTypeRef = beerType; // Create local variable for closure
                selectButton.onClick.AddListener(() => SelectBeerType(beerTypeRef));
            }
            else
            {
                // Show unlock panel for locked beers
                buttonObj.transform.Find("UnlockPanel").gameObject.SetActive(true);
                buttonObj.transform.Find("SelectButton").gameObject.SetActive(false);
                buttonObj.transform.Find("Selected").gameObject.SetActive(false);
                
                // Set unlock cost
                buttonObj.transform.Find("UnlockPanel/UnlockCost").GetComponent<TMP_Text>().text = "$" + beerType.unlockCost;
                
                // Value preview
                buttonObj.transform.Find("Value").GetComponent<TMP_Text>().text = "Value: $" + beerType.baseValue + " per perfect pour";
                
                // Add unlock button listener
                Button unlockButton = buttonObj.transform.Find("UnlockPanel/UnlockButton").GetComponent<Button>();
                BeerType beerTypeRef = beerType; // Create local variable for closure
                unlockButton.onClick.AddListener(() => UnlockBeerType(beerTypeRef));
            }
        }
    }
    
    private void UpdateUpgradeButtonStates()
    {
        for (int i = 0; i < availableUpgrades.Count && i < upgradeButtons.Count; i++)
        {
            Upgrade upgrade = availableUpgrades[i];
            GameObject buttonObj = upgradeButtons[i];
            
            // Update button texts
            buttonObj.transform.Find("Level").GetComponent<TMP_Text>().text = "Level: " + upgrade.currentLevel + "/" + upgrade.maxLevel;
            buttonObj.transform.Find("Cost").GetComponent<TMP_Text>().text = "$" + GetNextUpgradeCost(upgrade);
            
            // Disable button if max level or not enough money
            Button button = buttonObj.GetComponent<Button>();
            int cost = GetNextUpgradeCost(upgrade);
            
            if (upgrade.currentLevel >= upgrade.maxLevel)
            {
                button.interactable = false;
                buttonObj.transform.Find("Cost").GetComponent<TMP_Text>().text = "MAXED OUT";
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
        
        foreach (GameObject buttonObj in beerButtons)
        {
            string beerName = buttonObj.transform.Find("BeerName").GetComponent<TMP_Text>().text;
            
            // Find this beer type
            BeerType foundBeer = null;
            foreach (BeerType bt in CurrencyManager.instance.GetAvailableBeerTypes())
            {
                if (bt.beerName == beerName)
                {
                    foundBeer = bt;
                    break;
                }
            }
            
            if (foundBeer != null)
            {
                if (foundBeer.isUnlocked)
                {
                    // Update selected status
                    BeerType currentBeer = CurrencyManager.instance.GetCurrentBeerType();
                    if (foundBeer.beerName == currentBeer.beerName)
                    {
                        buttonObj.transform.Find("Selected").gameObject.SetActive(true);
                        buttonObj.transform.Find("SelectButton").gameObject.SetActive(false);
                    }
                    else
                    {
                        buttonObj.transform.Find("Selected").gameObject.SetActive(false);
                        buttonObj.transform.Find("SelectButton").gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Update unlock button state based on money
                    Button unlockButton = buttonObj.transform.Find("UnlockPanel/UnlockButton").GetComponent<Button>();
                    unlockButton.interactable = CurrencyManager.instance.GetCurrentMoney() >= foundBeer.unlockCost;
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
            
            // Refresh beer selection UI
            UpdateBeerButtonStates();
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
