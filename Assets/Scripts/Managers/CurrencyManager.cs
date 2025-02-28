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
        Debug.Log("CurrencyManager Awake called");

        if (instance == null)
        {
            instance = this;
            Debug.Log("CurrencyManager instance set");
        }
        else if (instance != this)
        {
            Debug.LogWarning("Multiple CurrencyManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        // Load saved money
        currentMoney = PlayerPrefs.GetInt("MONEY", 0);
        UpdateMoneyUI();

        // Check and initialize beer types if needed
        if (availableBeerTypes == null || availableBeerTypes.Length == 0)
        {
            Debug.LogWarning("No beer types defined, initializing defaults");
            InitializeDefaultBeerTypes();
        }
        else
        {
            Debug.Log("CurrencyManager has " + availableBeerTypes.Length + " beer types defined");
        }

        // Load unlock status for beer types
        LoadBeerUnlockStatus();

        // Start with first beer type or previously selected beer
        int selectedBeerIndex = PlayerPrefs.GetInt("SELECTED_BEER_INDEX", 0);
        if (availableBeerTypes.Length > 0)
        {
            if (selectedBeerIndex < availableBeerTypes.Length)
                SetBeerType(availableBeerTypes[selectedBeerIndex]);
            else
                SetBeerType(availableBeerTypes[0]);
        }
    }


    private void InitializeDefaultBeerTypes()
    {
        // Create and populate beer types array with defaults
        availableBeerTypes = new BeerType[3];

        // Create beer type 1 (always unlocked - starter beer)
        availableBeerTypes[0] = new BeerType
        {
            beerName = "Pale Lager",
            beerColor = new Color(0.9f, 0.7f, 0.2f, 1f),
            foamHeight = 0.8f,
            bubbleIntensity = 0.5f,
            baseValue = 10,
            isUnlocked = true,
            unlockCost = 0
        };

        // Create beer type 2
        availableBeerTypes[1] = new BeerType
        {
            beerName = "IPA",
            beerColor = new Color(0.8f, 0.45f, 0.1f, 1f),
            foamHeight = 1.0f,
            bubbleIntensity = 0.7f,
            baseValue = 25,
            isUnlocked = false,
            unlockCost = 50
        };

        // Create beer type 3
        availableBeerTypes[2] = new BeerType
        {
            beerName = "Stout",
            beerColor = new Color(0.3f, 0.2f, 0.1f, 1f),
            foamHeight = 1.2f,
            bubbleIntensity = 0.3f,
            baseValue = 50,
            isUnlocked = false,
            unlockCost = 100
        };

        // Log the initialization
        Debug.Log("Initialized " + availableBeerTypes.Length + " default beer types");

        // Force save the beer unlock status
        SaveBeerUnlockStatus();
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

    private void SaveBeerUnlockStatus()
    {
        if (availableBeerTypes == null) return;

        for (int i = 0; i < availableBeerTypes.Length; i++)
        {
            PlayerPrefs.SetInt("BEER_" + i + "_UNLOCKED", availableBeerTypes[i].isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void LoadBeerUnlockStatus()
    {
        if (availableBeerTypes == null) return;

        for (int i = 0; i < availableBeerTypes.Length; i++)
        {
            if (i < availableBeerTypes.Length)
            {
                availableBeerTypes[i].isUnlocked = PlayerPrefs.GetInt("BEER_" + i + "_UNLOCKED", i == 0 ? 1 : 0) == 1;
            }
        }
    }
    public void SetBeerType(BeerType beerType)
    {
        currentBeerType = beerType;

        // Save the selection
        for (int i = 0; i < availableBeerTypes.Length; i++)
        {
            if (availableBeerTypes[i] == beerType)
            {
                PlayerPrefs.SetInt("SELECTED_BEER_INDEX", i);
                break;
            }
        }

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
        if (jar == null || beerType == null)
        {
            Debug.LogWarning("UpdateJarLiquid: Jar or BeerType is null");
            return;
        }

        Debug.Log($"Updating jar '{jar.nameOfJar}' with beer color: {beerType.beerColor}");

        // First, ensure the sprite renderer is initialized
        if (jar.myLiquid != null)
        {
            // Make sure the liquid's SpriteRenderer is properly initialized
            if (jar.myLiquid.spriteRen == null)
            {
                jar.myLiquid.spriteRen = jar.myLiquid.GetComponent<SpriteRenderer>();
                Debug.Log("Had to get SpriteRenderer component");
            }

            if (jar.myLiquid.spriteRen != null)
            {
                // Update the color property
                Color oldColor = jar.myLiquid.spriteRen.color;
                jar.myLiquid.spriteRen.color = beerType.beerColor;
                Debug.Log($"Updated color from {oldColor} to {beerType.beerColor}");

                // If using a custom material/shader with properties
                Material beerMaterial = jar.myLiquid.spriteRen.material;
                if (beerMaterial != null)
                {
                    // Check if material has these properties and update them
                    if (beerMaterial.HasProperty("_BaseColor"))
                    {
                        beerMaterial.SetColor("_BaseColor", beerType.beerColor);
                        Debug.Log("Updated _BaseColor property on material");
                    }

                    if (beerMaterial.HasProperty("_BeerColor"))
                    {
                        beerMaterial.SetColor("_BeerColor", beerType.beerColor);
                        Debug.Log("Updated _BeerColor property on material");
                    }

                    // Update foam height if the property exists
                    if (beerMaterial.HasProperty("_FoamHeight"))
                    {
                        beerMaterial.SetFloat("_FoamHeight", beerType.foamHeight);
                        Debug.Log($"Updated foam height to {beerType.foamHeight}");
                    }

                    // Update bubble intensity if the property exists
                    if (beerMaterial.HasProperty("_BubbleIntensity"))
                    {
                        beerMaterial.SetFloat("_BubbleIntensity", beerType.bubbleIntensity);
                        Debug.Log($"Updated bubble intensity to {beerType.bubbleIntensity}");
                    }
                }
                else
                {
                    Debug.LogWarning("Beer material is null");
                }
            }
            else
            {
                Debug.LogError("Liquid SpriteRenderer is null after attempted initialization");
            }
        }
        else
        {
            Debug.LogError("Jar's myLiquid reference is null");
        }
    }

    // Add this method to ensure the current jar updates immediately when a beer is selected
    public void ForceUpdateCurrentJar()
    {
        if (GameManager.instance != null && GameManager.instance.currentJar != null && currentBeerType != null)
        {
            Debug.Log("Force updating current jar liquid to match selected beer");
            UpdateJarLiquid(GameManager.instance.currentJar, currentBeerType);
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
}

