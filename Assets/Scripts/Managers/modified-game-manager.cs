using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("JAR VALUES")]
    [HideInInspector] public Jar currentJar;
    [HideInInspector] public Vector2 centerJar;
    public bool isOutro;


    [Header("JAR ANIMATION VALUES")]
    public float animationDelay;
    public float xJarValue;
    public float xJarValueForBoot;
    public float xJarValueForMug;
    public float waitForNextJar;
    [HideInInspector] public bool canPlay = false;
    [HideInInspector] public bool canSiphonColor = false;


    [Header("LIQUID VALUES")]
    [SerializeField] private float riseAmount;
    private LiquidRise liquidRise;
    private GameObject ball;
    private float ballPosYStart;
    private Vector2 posGoal;
    private Vector2 posLiquid;


    [Header("UI ELEMENTS")]
    [HideInInspector] public int totalScore;
    private float porcentageCurrent;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text maxScoreText;
    [SerializeField] private TMP_Text porcentageText;
    [SerializeField] private TMP_Text styleText;
    public float timeDelayForColor;
    public GameObject outro;
    [SerializeField] private TMP_Text maxScoreOutroText;
    [SerializeField] private TMP_Text scoreOutroText;


    [Header("SCORE VALUES")]
    public float offsetBAD;
    public float offsetGOOD;
    public float offsetPERFECT;
    private bool isStartingGame = true;


    [Header("COMBO ELEMENTS")]
    [SerializeField] private GameObject bbbHolder;
    [SerializeField] private TMP_Text combo1Text;
    [SerializeField] private TMP_Text combo2Text;
    [SerializeField] private TMP_Text combo3Text;
    public Color comboOff;
    private int countCombo;

    [Header("LIFE ELEMENTS")]
    private int lifeCount = 3;
    private int maxLifeCount = 3;
    [SerializeField] private Image hurtPanel;
    [SerializeField] private TMP_Text life1Text;
    [SerializeField] private TMP_Text life2Text;
    [SerializeField] private TMP_Text life3Text;
    
    [Header("PROGRESSION SYSTEM")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private GameObject moneyEarnedPrefab;
    [SerializeField] private Transform moneyEarnedParent;
    [SerializeField] private Button shopButton;
    private float jarValueMultiplier = 1f;
    private float comboMultiplier = 1f;

    private void Awake()
    {
        centerJar = new Vector2(xJarValue, -2.7f);
        totalScore = 0;
        UpdateUI();
        instance = this;
        ball = GameObject.FindGameObjectWithTag("Ball");
        
        // Load progression values from PlayerPrefs
        LoadProgressionValues();
    }
    
    private void Start()
    {
        porcentageText.gameObject.SetActive(false);
        ReadComboCount();
        
        // Set up shop button
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OpenShopMenu);
        }
        
        // Initialize money display
        UpdateMoneyDisplay();
    }
    
    // Load progression-related values from PlayerPrefs
    private void LoadProgressionValues()
    {
        // Load upgraded values
        riseAmount = PlayerPrefs.GetFloat("PourSpeedBase", riseAmount);
        jarValueMultiplier = PlayerPrefs.GetFloat("JarValueMultiplier", 1f);
        comboMultiplier = PlayerPrefs.GetFloat("ComboMultiplier", 1f);
        
        // Load accuracy bonuses
        float goodRangeBonus = PlayerPrefs.GetFloat("GoodRangeBonus", 0f);
        float perfectRangeBonus = PlayerPrefs.GetFloat("PerfectRangeBonus", 0f);
        offsetGOOD += goodRangeBonus;
        offsetPERFECT += perfectRangeBonus;
        
        // Load max lives
        maxLifeCount = PlayerPrefs.GetInt("MaxLives", 3);
        lifeCount = maxLifeCount;
    }
    
    // Open the shop/upgrade menu
    public void OpenShopMenu()
    {
        if (canPlay && !isOutro && ShopManager.instance != null)
        {
            ShopManager.instance.OpenShop();
        }
    }
    
    // Update money display
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null && CurrencyManager.instance != null)
        {
            moneyText.text = "$" + CurrencyManager.instance.GetCurrentMoney().ToString();
        }
    }
    
    public void StartMugFunction()
    {
        StartCoroutine(PassTheOtherJar());
    }

    public void ChangeCurrentJar(Jar newJar)
    {
        ChangeRandomRise();
        currentJar = newJar;
        posLiquid = currentJar.startPosition;
        ballPosYStart = posLiquid.y;

        // Apply current beer type to the new jar
        if (CurrencyManager.instance != null)
        {
            BeerType currentBeer = CurrencyManager.instance.GetCurrentBeerType();
            if (currentBeer != null)
            {
                // If using CurrencyManager's update method
                CurrencyManager.instance.UpdateJarLiquid(newJar, currentBeer);

                // Or directly update here
                if (newJar.myLiquid != null && newJar.myLiquid.spriteRen != null)
                {
                    newJar.myLiquid.spriteRen.color = currentBeer.beerColor;
                    // If using material properties, update them here
                }
            }
        }
    }

    public void ClickInput()
    {
        if (currentJar != null)
        {
            liquidRise = currentJar.ReturnLiquidRise();
            liquidRise.RiseTheLiquid(riseAmount * Time.deltaTime);
            posGoal = liquidRise.ReturnGoalPosition();
            float newScale = liquidRise.ReturnSizeY();
            ball.transform.position = new Vector2(posLiquid.x, posLiquid.y + Mathf.Abs(newScale) +
                PoolManager.instance.poolPatern.position.y);
            Debug.Log(ReturnDistanceGoal());
        }
    }
    
    public void ButtonUpAction()
    {
        float distance = ReturnDistanceGoal();
        currentJar.foam.gameObject.SetActive(true);
        currentJar.foam.MoveFoam();
        StartCoroutine(PassTheOtherJar());
        ReadComboCount();
    }
    
    IEnumerator PassTheOtherJar()
    {
        yield return new WaitForSeconds(0.4f);
        if (currentJar != null)
        {
            ScoreCalculator();
            StartCoroutine(AnimationJar(currentJar.gameObject, false));
            yield return new WaitForSeconds(0.6f);
            GameObject go = PoolManager.instance.RequestJar();
            PoolManager.instance.ChangeStartPosition(go);
            StartCoroutine(AnimationJar(go, true));
            ColorManager.instance.ReturnSiphonImage().DOColor(ColorManager.instance.firstColor, timeDelayForColor);
            go.GetComponent<Jar>().ResetValues();
            ChangeCurrentJar(go.GetComponent<Jar>());
        }
        else if (currentJar == null)
        {
            GameObject go = PoolManager.instance.RequestJar();
            PoolManager.instance.ChangeStartPosition(go);
            ChangeCurrentJar(go.GetComponent<Jar>());
            StartCoroutine(AnimationJar(currentJar.gameObject, true));
            ColorManager.instance.ReturnSiphonImage().DOColor(ColorManager.instance.firstColor, timeDelayForColor);
            yield return new WaitForSeconds(0.6f);
            isStartingGame = false;
        }
    }
    
    void ChangeRandomRise()
    {
        // Modify to include the base pour speed from PlayerPrefs
        float basePourSpeed = PlayerPrefs.GetFloat("PourSpeedBase", 6f);
        float random = Random.Range(basePourSpeed - 2f, basePourSpeed + 2f);
        riseAmount = random;
    }
    
    IEnumerator AnimationJar(GameObject go, bool enter)
    {
        if (enter)
        {
            if (go.GetComponent<Jar>().jarType == JarType.Mug)
            {
                go.transform.DOMoveX(xJarValueForMug, animationDelay).SetEase(Ease.OutBack);
            }
            else if (go.GetComponent<Jar>().jarType == JarType.Boot)
            {
                go.transform.DOMoveX(xJarValueForBoot, animationDelay).SetEase(Ease.OutBack);
            }
            else
            {
                go.transform.DOMoveX(xJarValue, animationDelay).SetEase(Ease.OutBack);
            }
            yield return new WaitForSeconds(animationDelay + 0.5f);
            canPlay = true;
            InputManager.instance.ResetState(false);
            yield return null;
        }
        if (!enter)
        {
            canPlay = false;
            InputManager.instance.ResetState(true);
            go.transform.DOMoveX(-15, animationDelay).SetEase(Ease.InBack);
            yield return new WaitForSeconds(animationDelay);
            go.SetActive(false);
            yield return null;
        }
    }
    
    public float ReturnDistanceGoal()
    {
        return Mathf.Abs(posGoal.y - ball.transform.position.y);
    }

    public float ReturnPosOfTopLiquid()
    {
        return ball.transform.position.y;
    }
    
    public void SiphonColorGrading()
    {
        if (!canSiphonColor) { return; }
        float dis = ReturnDistanceGoal();
        Image siphon = ColorManager.instance.ReturnSiphonImage();
        if (dis >= offsetBAD)
        {
            siphon.DOColor(ColorManager.instance.colorBad, timeDelayForColor);
            UpdateStyleText(0);
        }
        else if (dis < offsetBAD && dis >= offsetGOOD)
        {
            siphon.DOColor(ColorManager.instance.colorGood, timeDelayForColor);
            UpdateStyleText(1);
        }
        else if (dis < offsetGOOD && dis >= offsetPERFECT)
        {
            UpdateStyleText(2);
            siphon.DOColor(ColorManager.instance.colorPerfect, timeDelayForColor);
        }
    }
    
    public enum JarType
    {
        Mug, Cup, Boot, Long
    }
    
    public void AddPoints(int pointsToAdd)
    {
        totalScore += pointsToAdd;
        StartCoroutine(PorcentageScoreAnimation());
        UpdateUI();
    }
    
    void UpdateUI()
    {
        pointsText.text = totalScore.ToString();
        maxScoreText.text = "Max Score: " + PlayerPrefs.GetInt("MAXSCORE", 0);
        porcentageText.text = porcentageCurrent.ToString("00.00") + "%";
        
        // Update money display
        UpdateMoneyDisplay();
    }
    
    IEnumerator PorcentageScoreAnimation()
    {
        porcentageText.gameObject.SetActive(true);
        porcentageText.transform.DOScale(1.6f, animationDelay / 3).SetLoops(2, LoopType.Yoyo);
        yield return new WaitForSeconds(animationDelay + 0.5f);
        porcentageText.gameObject.SetActive(false);
        porcentageText.transform.DOScale(1f, 0f);
    }
    
    void ScoreCalculator()
    {
        float porcentage = 100 - (ReturnDistanceGoal() * 100);
        porcentageCurrent = porcentage;
        
        // Get beer value multiplier
        float beerValueMultiplier = 1f;
        if (CurrencyManager.instance != null && CurrencyManager.instance.GetCurrentBeerType() != null)
        {
            beerValueMultiplier = CurrencyManager.instance.GetCurrentBeerType().baseValue / 100f;
        }
        
        // Apply jar value multiplier from upgrades
        int basePoints = Mathf.RoundToInt(currentJar.points * jarValueMultiplier);
        
        // Calculate score with enhanced combo system
        int addScore = (int)(basePoints * porcentage / 100);
        int realAddScore = Mathf.RoundToInt(addScore * (1 + (countCombo * comboMultiplier / 10)));
        
        // Calculate money earned
        int moneyEarned = Mathf.RoundToInt(realAddScore * beerValueMultiplier);
        
        // Show money earned
        ShowMoneyEarned(moneyEarned);
        
        // Add money to currency manager
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddMoney(moneyEarned);
        }
        
        // Add points for score tracking
        AddPoints(realAddScore);

        // CHECK IF IS MAXSCORE
        int maxScore = PlayerPrefs.GetInt("MAXSCORE", 0);
        if (totalScore > maxScore)
        {
            PlayerPrefs.SetInt("MAXSCORE", totalScore);
        }
        UpdateUI();
    }
    
    // Show money earned animation
    void ShowMoneyEarned(int amount)
    {
        if (moneyEarnedPrefab != null && moneyEarnedParent != null)
        {
            GameObject moneyObj = Instantiate(moneyEarnedPrefab, moneyEarnedParent);
            TMP_Text moneyTextComponent = moneyObj.GetComponent<TMP_Text>();
            
            if (moneyTextComponent != null)
            {
                moneyTextComponent.text = "+$" + amount;
                
                // Set initial color
                moneyTextComponent.color = new Color(0.2f, 0.9f, 0.2f);
                
                // Animate
                Sequence sequence = DOTween.Sequence();
                sequence.Append(moneyObj.transform.DOLocalMoveY(moneyObj.transform.localPosition.y + 100f, 1f));
                sequence.Join(moneyTextComponent.DOFade(0, 1f));
                sequence.OnComplete(() => Destroy(moneyObj));
            }
        }
    }
    
    void UpdateStyleText(int index)
    {
        //BAD 0, GOOD 1, PERFECT 2

        if (index == 0)
        {
            styleText.text = "Bad";
            styleText.color = Color.red;
        }
        else if (index == 1)
        {
            styleText.text = "Good";
            styleText.color = Color.yellow;
        }
        else if (index == 2)
        {
            styleText.text = "Perfect";
            styleText.color = Color.green;
        }
    }
    
    void ReadComboCount()
    {
        float dis = ReturnDistanceGoal();
        if (dis >= offsetBAD && !isStartingGame)
        {
            DamageLifeCount();
            AudioManager.instance.PlaySound("wrong");
            countCombo = 0;
        }
        else if (dis < offsetBAD && dis >= offsetGOOD)
        {
            AudioManager.instance.PlaySound("correct");
            countCombo += 1;
        }
        else if (dis < offsetGOOD && dis >= offsetPERFECT)
        {
            AudioManager.instance.PlaySound("correct");
            countCombo += 1;
        }
        ComboFunction();
    }
    
    void ComboFunction()
    {
        if (countCombo == 1)
        {
            bbbHolder.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -105f), animationDelay).SetEase(Ease.OutCubic);
            //Enter the sign
            return;
        }
        else if (countCombo == 2)
        {
            combo1Text.color = Color.red;
            //Encender 1 luz
            return;
        }
        else if (countCombo == 3)
        {
            combo2Text.color = Color.red;
            //Encender 2 luz
            return;
        }
        else if (countCombo == 4)
        {
            combo3Text.color = Color.red;
            //Encender 3 luces
            return;
        }
        else if (countCombo == 5)
        {
            AudioManager.instance.ChangePitch(1.3f, "background1");
            return;
        }
        else if (countCombo == 0)
        {
            bbbHolder.GetComponent<RectTransform>().DOAnchorPos(new Vector2(200, -105f), animationDelay);
            AudioManager.instance.ChangePitch(1f, "background1");
            combo1Text.color = comboOff;
            combo2Text.color = comboOff;
            combo3Text.color = comboOff;
            //Reiniciar valores
            return;
        }
    }
    
    void DamageLifeCount()
    {
        StartCoroutine(HurtAnimation());
        lifeCount--;
        if (lifeCount == maxLifeCount-1)
        {
            life3Text.color = comboOff;
        }
        if (lifeCount == maxLifeCount-2)
        {
            life2Text.color = comboOff;
        }
        if (lifeCount <= 0)
        {
            life1Text.color = comboOff;
            canPlay = false;
            
            // Update outro with both score and money
            maxScoreOutroText.text = "Max Score: " + PlayerPrefs.GetInt("MAXSCORE", 0);
            scoreOutroText.text = "Score: " + totalScore.ToString();
            
            // Add money display to outro if available
            if (CurrencyManager.instance != null)
            {
                // You may need to add a reference to a TMP_Text for this in the inspector
                // moneyOutroText.text = "Money: $" + CurrencyManager.instance.GetCurrentMoney();
            }
            
            StartCoroutine(OutroAnim());
        }
    }
    
    IEnumerator OutroAnim()
    {
        AudioManager.instance.ChangePitch(0.5f, "background1");
        yield return new WaitForSeconds(1f);
        outro.SetActive(true);
        outro.GetComponent<CanvasGroup>().DOFade(1, animationDelay / 2);
        isOutro = true;
    }
    
    IEnumerator HurtAnimation()
    {
        hurtPanel.gameObject.SetActive(true);
        hurtPanel.DOFade(1f, animationDelay / 4);
        yield return new WaitForSeconds(animationDelay / 4);
        hurtPanel.DOFade(0f, animationDelay / 4);
        yield return new WaitForSeconds(animationDelay / 4);
        hurtPanel.gameObject.SetActive(false);
    }
}
