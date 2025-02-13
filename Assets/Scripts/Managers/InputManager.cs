using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    private bool isFinish;
    private GameObject liquidSiphon;
    private bool isPouring = false;
    private void Awake()
    {
        instance = this;
        liquidSiphon = GameObject.FindGameObjectWithTag("LiquidSiphon");
        liquidSiphon.SetActive(false);
    }
    private void Update()
    {
        if (GameManager.instance.isOutro) { return; }
        if (Input.GetKey(KeyCode.Space) && !isFinish && GameManager.instance.canPlay)
        {
            GameManager.instance.canSiphonColor = true;
            GameManager.instance.ClickInput();
            liquidSiphon.SetActive(true);
            if (!isPouring)
            {
                isPouring=true;
                AudioManager.instance.PlaySound("pour");
            }
        }
        if (Input.GetKeyUp(KeyCode.Space) && GameManager.instance.canPlay)
        {
            AudioManager.instance.StopSound("pour");
            GameManager.instance.canSiphonColor = false;
            GameManager.instance.canPlay = false;
            GameManager.instance.ButtonUpAction();
            liquidSiphon.SetActive(false);
            isFinish = true;
            isPouring = false;
        }
        if (GameManager.instance.canPlay && GameManager.instance.canSiphonColor)
        {
            GameManager.instance.SiphonColorGrading();
        }
    }

    public void ResetState(bool state)
    {
        isFinish = state;
    }
}
