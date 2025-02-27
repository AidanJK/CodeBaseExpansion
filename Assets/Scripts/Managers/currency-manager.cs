using UnityEngine;
using TMPro;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;
    
    [Header("Currency Settings")]
    [SerializeField] private int currentMoney;
    [SerializeField] private TMP_Text moneyText;
    
    [Header("Beer Types")]
    [SerializeField] private BeerType[] availableBeerTypes;
    [SerializeField] private BeerType currentBeerType;
    [SerializeField] private TMP_Text beerTypeText;
    
    // Event for when money changes
    public event Action<int> OnMoneyChanged;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
        // Load saved money
        currentMoney = PlayerPrefs.GetInt("MONEY", 0);
        UpdateMoneyUI();
        
        // Start with first beer type
        if (availableBeerTypes.Length > 0)
            SetBeerType(availableBeerTypes[0]);
    }
    
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        PlayerPrefs.SetInt("MONEY", currentMoney);
        UpdateMoneyUI();
        
        // Trigger event
        OnMoneyChanged?.Invoke(currentMoney);
    }
    
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            PlayerPrefs.SetInt("MONEY", currentMoney);
            UpdateMoneyUI();
            
            // Trigger event
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        return false;
    }
    
    public int GetCurrentMoney()
    {
        return currentMoney;
    }
    
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = "$" + currentMoney.ToString();
    }

    public void SetBeerType(BeerType beerType)
    {
        currentBeerType = beerType;

        // Update the current jar's liquid if one exists
        if (GameManager.instance != null && GameManager.instance.currentJar != null)
        {
            UpdateJarLiquid(GameManager.instance.currentJar, beerType);
        }

        // Update UI
        if (beerTypeText != null)
            beerTypeText.text = beerType.beerName;
    }

    // Method to update the jar's liquid appearance
    public void UpdateJarLiquid(Jar jar, BeerType beerType)
    {
        if (jar.myLiquid != null && jar.myLiquid.spriteRen != null)
        {
            // If using just color property
            jar.myLiquid.spriteRen.color = beerType.beerColor;

            // If using a material/shader
            Material beerMaterial = jar.myLiquid.spriteRen.material;
            if (beerMaterial != null)
            {
                // Update material properties based on beer type
                beerMaterial.SetColor("_BeerColor", beerType.beerColor);
                // Add other material property updates as needed
            }
        }
    }
    public BeerType GetCurrentBeerType()
    {
        return currentBeerType;
    }
    
    public BeerType[] GetAvailableBeerTypes()
    {
        return availableBeerTypes;
    }
}

[System.Serializable]
public class BeerType
{
    public string beerName;
    public Color beerColor;    // The main color parameter for your shader
    public float foamHeight;   // If your shader has foam properties
    public float bubbleIntensity; // For bubble effects in the shader
    public int baseValue;      // Economic value of this beer
    public bool isUnlocked;
    public int unlockCost;

    private void UpdateBeerShader(BeerType beerType)
    {
        // Find current jar and update its liquid material
        Jar currentJar = GameManager.instance.currentJar;
        if (currentJar != null && currentJar.myLiquid != null)
        {
            // Assuming myLiquid has a renderer with the beer material
            Renderer liquidRenderer = currentJar.myLiquid.GetComponent<Renderer>();
            if (liquidRenderer != null)
            {
                // Apply beer color to the main shader color property
                liquidRenderer.material.SetColor("_BeerColor", beerType.beerColor);

                // If your shader has foam height parameters
                liquidRenderer.material.SetFloat("_FoamHeight", beerType.foamHeight);

                // If your shader has bubble intensity
                liquidRenderer.material.SetFloat("_BubbleIntensity", beerType.bubbleIntensity);
            }
        }
    }

    private void InitializeBeerTypes()
    {
        availableBeerTypes = new BeerType[5];

        // Pale Lager - Light, clear, moderately bubbly
        availableBeerTypes[0] = new BeerType
        {
            beerName = "Pale Lager",
            beerColor = new Color(0.9f, 0.7f, 0.2f), // Light amber
            foamHeight = 0.8f,
            bubbleIntensity = 0.5f,
            baseValue = 10,
            isUnlocked = true,
            unlockCost = 0
        };

        // IPA - Darker amber, more foam
        availableBeerTypes[1] = new BeerType
        {
            beerName = "IPA",
            beerColor = new Color(0.8f, 0.4f, 0.2f), // Amber
            foamHeight = 1.0f,
            bubbleIntensity = 0.7f,
            baseValue = 25,
            isUnlocked = false,
            unlockCost = 500
        };

        // Stout - Dark, thick, creamy foam
        availableBeerTypes[2] = new BeerType
        {
            beerName = "Stout",
            beerColor = new Color(0.2f, 0.15f, 0.1f), // Dark brown
            foamHeight = 1.2f,
            bubbleIntensity = 0.3f, // Less bubbly
            baseValue = 50,
            isUnlocked = false,
            unlockCost = 1000
        };
    }

