using UnityEngine;
using DG.Tweening;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    
    [Header("Shop UI Elements")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject beerSelectionPanel;
    
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
        // Don't allow opening if game is paused for other reasons
        if (!GameManager.instance.canPlay && !isShopOpen) return;
        
        // Pause game (using flags instead of Time.timeScale to avoid conflicts)
        GameManager.instance.canPlay = false;
        InputManager.instance.ResetState(true);
        
        // Show shop panel
        shopPanel.SetActive(true);
        shopPanel.GetComponent<CanvasGroup>().alpha = 0;
        shopPanel.GetComponent<CanvasGroup>().DOFade(1, 0.3f);
        
        // Default to upgrades panel
        ShowUpgradesPanel();
        
        isShopOpen = true;
        
        // Play sound
        AudioManager.instance.PlaySound("menu");
    }
    
    public void CloseShop()
    {
        // Unpause game
        GameManager.instance.canPlay = true;
        InputManager.instance.ResetState(false);
        
        // Hide panel with animation
        shopPanel.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => {
            shopPanel.SetActive(false);
        });
        
        isShopOpen = false;
        
        // Play sound
        AudioManager.instance.PlaySound("menu");
    }
    
    public void ShowUpgradesPanel()
    {
        upgradesPanel.SetActive(true);
        beerSelectionPanel.SetActive(false);
        
        // Refresh upgrade UI
        UpgradeSystem.instance.RefreshUpgradeUI();
    }
    
    public void ShowBeerSelectionPanel()
    {
        upgradesPanel.SetActive(false);
        beerSelectionPanel.SetActive(true);
        
        // Refresh beer selection UI
        UpgradeSystem.instance.RefreshBeerUI();
    }
    
    // Helper method to check if shop can be opened
    public bool CanOpenShop()
    {
        return GameManager.instance.canPlay && !GameManager.instance.isOutro;
    }
}
