using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("Shop UI Elements")]
    [SerializeField] private GameObject shopPanel; // Your "ShopPanel" GameObject
    [SerializeField] private GameObject beerSelectPanel; // Your "BeerSelectPanel" GameObject
    [SerializeField] private GameObject upgradeStorePanel; // Your "UpgradeStore" GameObject
    [SerializeField] private Button closeShopButton; // Your "CloseShopButton" GameObject

    [Header("Tab Buttons")]
    [SerializeField] private Button beerStoreTabButton; // "Beer Store" button
    [SerializeField] private Button upgradeStoreTabButton; // "Upgrade Store" button

    private bool isShopOpen = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Initialize panels as closed
        if (shopPanel != null)
            shopPanel.SetActive(false);

        // Set up button listeners
        if (closeShopButton != null)
        {
            closeShopButton.onClick.RemoveAllListeners();
            closeShopButton.onClick.AddListener(CloseShop);
            Debug.Log("Close button listener set up");
        }

        if (beerStoreTabButton != null)
        {
            beerStoreTabButton.onClick.RemoveAllListeners();
            beerStoreTabButton.onClick.AddListener(ShowBeerSelectionPanel);
            Debug.Log("Beer tab button listener set up");
        }

        if (upgradeStoreTabButton != null)
        {
            upgradeStoreTabButton.onClick.RemoveAllListeners();
            upgradeStoreTabButton.onClick.AddListener(ShowUpgradesPanel);
            Debug.Log("Upgrade tab button listener set up");
        }
    }

    private void Update()
    {
        // Don't process input if game is in intro or outro state
        if (GameManager.instance.isOutro) return;

        // Shop toggle key - only works when game is not paused for other reasons
        if (Input.GetKeyDown(KeyCode.U) && GameManager.instance.canPlay)
        {
            ToggleShop();
        }

        // Allow closing shop with escape key
        if (Input.GetKeyDown(KeyCode.Escape) && isShopOpen)
        {
            CloseShop();
        }
    }

    public void ToggleShop()
    {
        if (isShopOpen)
            CloseShop();
        else
            OpenShop();
    }

    public void OpenShop()
    {
        Debug.Log("Opening shop");

        // Don't allow opening if game is paused for other reasons
        if (!GameManager.instance.canPlay && !isShopOpen) return;

        // Pause game
        GameManager.instance.canPlay = false;
        InputManager.instance.ResetState(true);

        // Show shop panel
        shopPanel.SetActive(true);

        // Default to showing the beer selection panel
        ShowBeerSelectionPanel();

        isShopOpen = true;

        // Play sound if available
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySound("menu");
    }

    public void CloseShop()
    {
        Debug.Log("Closing shop");

        // Unpause game
        GameManager.instance.canPlay = true;
        InputManager.instance.ResetState(false);

        // Hide panel immediately
        shopPanel.SetActive(false);

        isShopOpen = false;

        // Play sound if available
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySound("menu");
    }

    public void ShowBeerSelectionPanel()
    {
        Debug.Log("Showing Beer Selection Panel");

        // Make sure panels exist
        if (beerSelectPanel == null || upgradeStorePanel == null)
        {
            Debug.LogError("Beer select panel or upgrade store panel reference is missing!");
            return;
        }

        // Show beer panel, hide upgrade panel
        beerSelectPanel.SetActive(true);
        upgradeStorePanel.SetActive(false);

        // Visual feedback for selected tab (optional)
        if (beerStoreTabButton != null)
            beerStoreTabButton.interactable = false;
        if (upgradeStoreTabButton != null)
            upgradeStoreTabButton.interactable = true;

        // Refresh beer UI if needed
        if (UpgradeSystem.instance != null)
            UpgradeSystem.instance.RefreshBeerUI();
    }

    public void ShowUpgradesPanel()
    {
        Debug.Log("Showing Upgrades Panel");

        // Make sure panels exist
        if (beerSelectPanel == null || upgradeStorePanel == null)
        {
            Debug.LogError("Beer select panel or upgrade store panel reference is missing!");
            return;
        }

        // Show upgrade panel, hide beer panel
        upgradeStorePanel.SetActive(true);
        beerSelectPanel.SetActive(false);

        // Visual feedback for selected tab (optional)
        if (beerStoreTabButton != null)
            beerStoreTabButton.interactable = true;
        if (upgradeStoreTabButton != null)
            upgradeStoreTabButton.interactable = false;

        // Refresh upgrade UI if needed
        if (UpgradeSystem.instance != null)
            UpgradeSystem.instance.RefreshUpgradeUI();
    }

    // Helper method to check if shop can be opened
    public bool CanOpenShop()
    {
        return GameManager.instance.canPlay && !GameManager.instance.isOutro;
    }
}