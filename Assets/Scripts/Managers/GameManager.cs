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
    public float timeDelayForColor;
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

    [Header("PROGRESSION SYSTEM")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private GameObject moneyEarnedPrefab;
    [SerializeField] private Transform moneyEarnedParent;
    [SerializeField] private Button shopButton;
    private float jarValueMultiplier = 1f;
    private float comboMultiplier = 1f;

    private void Awake()
    {
        // Set up initial game state
        centerJar = new Vector2(xJarValue, -2.7f);
        totalScore = 0;
        UpdateUI();
        instance = this;
        ball = GameObject.FindGameObjectWithTag("Ball");

        // Load saved progression values
        LoadProgressionValues();
    }

    private void Start()
    {
        // Initialize combo display
        ReadComboCount();

        // Set up shop button
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OpenShopMenu);
        }

        // Update money display
        UpdateMoneyDisplay();
    }

    // Load upgrades and progression values from saved data
    private void LoadProgressionValues()
    {
        // Load base pour speed
        riseAmount = PlayerPrefs.GetFloat("PourSpeedBase", riseAmount);

        // Load multipliers for jar value and combo
        jarValueMultiplier = PlayerPrefs.GetFloat("JarValueMultiplier", 1f);
        comboMultiplier = PlayerPrefs.GetFloat("ComboMultiplier", 1f);

        // Adjust accuracy ranges
        float goodRangeBonus = PlayerPrefs.GetFloat("GoodRangeBonus", 0f);
        float perfectRangeBonus = PlayerPrefs.GetFloat("PerfectRangeBonus", 0f);
        offsetGOOD += goodRangeBonus;
        offsetPERFECT += perfectRangeBonus;

        // Set max lives
        maxLifeCount = PlayerPrefs.GetInt("MaxLives", 3);
        lifeCount = maxLifeCount;
    }

    // Open the upgrade/shop menu
    public void OpenShopMenu()
    {
        if (canPlay && !isOutro && ShopManager.instance != null)
        {
            ShopManager.instance.OpenShop();
        }
    }

    // Update displayed money amount
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null && CurrencyManager.instance != null)
        {
            moneyText.text = "$" + CurrencyManager.instance.GetCurrentMoney().ToString();
        }
    }

    // Start the process of passing to the next jar
    public void StartMugFunction()
    {
        StartCoroutine(PassTheOtherJar());
    }

    // Change the current active jar
    public void ChangeCurrentJar(Jar newJar)
    {
        // Randomize pour speed slightly
        ChangeRandomRise();

        // Set new current jar
        currentJar = newJar;
        posLiquid = currentJar.startPosition;
        ballPosYStart = posLiquid.y;

        // Apply current beer type to the new jar
        if (CurrencyManager.instance != null)
        {
            BeerType currentBeer = CurrencyManager.instance.GetCurrentBeerType();
            if (currentBeer != null)
            {
                // Update jar liquid color and properties
                CurrencyManager.instance.UpdateJarLiquid(newJar, currentBeer);
            }
        }
    }

    // Handle pouring liquid when player holds the button
    public void ClickInput()
    {
        if (currentJar != null)
        {
            // Get liquid rise component
            liquidRise = currentJar.ReturnLiquidRise();

            // Increase liquid height based on pour speed
            liquidRise.RiseTheLiquid(riseAmount * Time.deltaTime);

            // Get goal position and update ball position
            posGoal = liquidRise.ReturnGoalPosition();
            float newScale = liquidRise.ReturnSizeY();
            ball.transform.position = new Vector2(posLiquid.x, posLiquid.y + Mathf.Abs(newScale) +
                PoolManager.instance.poolPatern.position.y);

            Debug.Log(ReturnDistanceGoal());
        }
    }

    // Handle actions when player releases the pour button
    public void ButtonUpAction()
    {
        // Calculate pour accuracy
        float distance = ReturnDistanceGoal();

        // Show foam at liquid top
        currentJar.foam.gameObject.SetActive(true);
        currentJar.foam.MoveFoam();

        // Move to next jar
        StartCoroutine(PassTheOtherJar());

        // Update combo display
        ReadComboCount();
    }

    // Coroutine to handle jar transition
    IEnumerator PassTheOtherJar()
    {
        yield return new WaitForSeconds(0.4f);
        if (currentJar != null)
        {
            // Calculate score for the pour
            ScoreCalculator();

            // Animate current jar out
            StartCoroutine(AnimationJar(currentJar.gameObject, false));
            yield return new WaitForSeconds(0.6f);

            // Request and setup next jar
            GameObject go = PoolManager.instance.RequestJar();
            PoolManager.instance.ChangeStartPosition(go);
            StartCoroutine(AnimationJar(go, true));

            // Reset siphon color
            ColorManager.instance.ReturnSiphonImage().DOColor(ColorManager.instance.firstColor, timeDelayForColor);

            // Reset jar liquid
            go.GetComponent<Jar>().ResetValues();

            // Change to new jar
            ChangeCurrentJar(go.GetComponent<Jar>());
        }
        else if (currentJar == null)
        {
            // First jar setup
            GameObject go = PoolManager.instance.RequestJar();
            PoolManager.instance.ChangeStartPosition(go);
            ChangeCurrentJar(go.GetComponent<Jar>());
            StartCoroutine(AnimationJar(currentJar.gameObject, true));
            ColorManager.instance.ReturnSiphonImage().DOColor(ColorManager.instance.firstColor, timeDelayForColor);
            yield return new WaitForSeconds(0.6f);
            isStartingGame = false;
        }
    }

    // Randomize pour speed with slight variation
    void ChangeRandomRise()
    {
        // Get base pour speed from upgrades
        float basePourSpeed = PlayerPrefs.GetFloat("PourSpeedBase", 6f);
        float random = Random.Range(basePourSpeed - 2f, basePourSpeed + 2f);
        riseAmount = random;
    }

    // Animate jar entering or leaving the screen
    IEnumerator AnimationJar(GameObject go, bool enter)
    {
        if (enter)
        {
            // Move jar to different positions based on type
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

            // Wait and enable player interaction
            yield return new WaitForSeconds(animationDelay + 0.5f);
            canPlay = true;
            InputManager.instance.ResetState(false);
            yield return null;
        }
        if (!enter)
        {
            // Disable player interaction and move jar off-screen
            canPlay = false;
            InputManager.instance.ResetState(true);
            go.transform.DOMoveX(-15, animationDelay).SetEase(Ease.InBack);
            yield return new WaitForSeconds(animationDelay);
            go.SetActive(false);
            yield return null;
        }
    }

    // Calculate distance between ball and goal position
    public float ReturnDistanceGoal()
    {
        return Mathf.Abs(posGoal.y - ball.transform.position.y);
    }

    // Get current liquid top position
    public float ReturnPosOfTopLiquid()
    {
        return ball.transform.position.y;
    }

    // Change siphon color based on pour accuracy
    public void SiphonColorGrading()
    {
        if (!canSiphonColor) { return; }

        float dis = ReturnDistanceGoal();
        Image siphon = ColorManager.instance.ReturnSiphonImage();

        // Color changes based on accuracy
        if (dis >= offsetBAD)
        {
            siphon.DOColor(ColorManager.instance.colorBad, timeDelayForColor);
        }
        else if (dis < offsetBAD && dis >= offsetGOOD)
        {
            siphon.DOColor(ColorManager.instance.colorGood, timeDelayForColor);
        }
        else if (dis < offsetGOOD && dis >= offsetPERFECT)
        {
            siphon.DOColor(ColorManager.instance.colorPerfect, timeDelayForColor);
        }
    }

    // Jar type enum
    public enum JarType
    {
        Mug, Cup, Boot, Long
    }

    // Add points to total score
    public void AddPoints(int pointsToAdd)
    {
        totalScore += pointsToAdd;
        UpdateUI();
    }

    // Update UI elements
    void UpdateUI()
    {
        // Update money display
        UpdateMoneyDisplay();
    }

    // Calculate score based on pour accuracy
    void ScoreCalculator()
    {
        // Calculate pour accuracy percentage
        float porcentage = 100 - (ReturnDistanceGoal() * 100);
        porcentageCurrent = porcentage;

        // Get beer value multiplier
        float beerValueMultiplier = 1f;
        if (CurrencyManager.instance != null && CurrencyManager.instance.GetCurrentBeerType() != null)
        {
            beerValueMultiplier = CurrencyManager.instance.GetCurrentBeerType().baseValue / 100f;
        }

        // Apply jar value multiplier
        int basePoints = Mathf.RoundToInt(currentJar.points * jarValueMultiplier);

        // Calculate score with combo system
        int addScore = (int)(basePoints * porcentage / 100);
        int realAddScore = Mathf.RoundToInt(addScore * (1 + (countCombo * comboMultiplier / 10)));

        // Calculate money earned
        int moneyEarned = Mathf.RoundToInt(realAddScore * beerValueMultiplier);

        // Show money earned animation
        ShowMoneyEarned(moneyEarned);

        // Add money to currency
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddMoney(moneyEarned);
        }

        // Add points to total score
        AddPoints(realAddScore);

        // Update max score if needed
        int maxScore = PlayerPrefs.GetInt("MAXSCORE", 0);
        if (totalScore > maxScore)
        {
            PlayerPrefs.SetInt("MAXSCORE", totalScore);
        }
        UpdateUI();
    }

    // Show money earned animation
    public void ShowMoneyEarned(int amount)
    {
        if (moneyEarnedPrefab != null && moneyEarnedParent != null)
        {
            // Instantiate money text
            GameObject moneyObj = Instantiate(moneyEarnedPrefab, moneyEarnedParent);
            TMP_Text moneyTextComponent = moneyObj.GetComponent<TMP_Text>();

            if (moneyTextComponent != null)
            {
                // Set money text and animate
                moneyTextComponent.text = "+$" + amount;
                moneyTextComponent.color = new Color(0.2f, 0.9f, 0.2f);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(moneyObj.transform.DOLocalMoveY(moneyObj.transform.localPosition.y + 100f, 1f));
                sequence.Join(moneyTextComponent.DOFade(0, 1f));
                sequence.OnComplete(() => Destroy(moneyObj));
            }
        }
    }

    // Calculate and update combo
    void ReadComboCount()
    {
        float dis = ReturnDistanceGoal();

        // Handle failed pour
        if (dis >= offsetBAD && !isStartingGame)
        {
            DamageLifeCount();
            AudioManager.instance.PlaySound("wrong");
            countCombo = 0;
        }
        // Handle good pours
        else if (dis < offsetBAD && dis >= offsetGOOD)
        {
            AudioManager.instance.PlaySound("correct");
            countCombo += 1;
        }
        // Handle perfect pours
        else if (dis < offsetGOOD && dis >= offsetPERFECT)
        {
            AudioManager.instance.PlaySound("correct");
            countCombo += 1;
        }

        // Update combo display
        ComboFunction();
    }

    // Manage combo display and effects
    void ComboFunction()
    {
        // Show combo holder
        if (countCombo == 1)
        {
            bbbHolder.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -105f), animationDelay).SetEase(Ease.OutCubic);
            return;
        }
        // Highlight first combo indicator
        else if (countCombo == 2)
        {
            combo1Text.color = Color.red;
            return;
        }
        // Highlight second combo indicator
        else if (countCombo == 3)
        {
            combo2Text.color = Color.red;
            return;
        }
        // Highlight third combo indicator
        else if (countCombo == 4)
        {
            combo3Text.color = Color.red;
            return;
        }
        // Increase background music pitch at max combo
        else if (countCombo == 5)
        {
            AudioManager.instance.ChangePitch(1.3f, "background1");
            return;
        }
        // Reset combo display
        else if (countCombo == 0)
        {
            // Move combo holder off-screen
            bbbHolder.GetComponent<RectTransform>().DOAnchorPos(new Vector2(200, -105f), animationDelay);

            // Reset music pitch
            AudioManager.instance.ChangePitch(1f, "background1");

            // Reset combo indicator colors
            combo1Text.color = comboOff;
            combo2Text.color = comboOff;
            combo3Text.color = comboOff;
            return;
        }
    }

    // Reduce player life count
    void DamageLifeCount()
    {
        // Show hurt animation
        StartCoroutine(HurtAnimation());

        // Stop pouring
        if (InputManager.instance != null)
        {
            InputManager.instance.ResetState(true);
        }

        // Play error sound
        AudioManager.instance.PlaySound("wrong");

        // Brief pause before allowing new input
        StartCoroutine(BriefPause());

        // Reset combo
        countCombo = 0;
    }

    // Pause game briefly after mistake
    IEnumerator BriefPause()
    {
        canPlay = false;
        yield return new WaitForSeconds(0.5f);
        canPlay = true;
    }

    // Animate hurt effect
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