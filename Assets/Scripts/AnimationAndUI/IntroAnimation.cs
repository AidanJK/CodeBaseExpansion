using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IntroAnimation : MonoBehaviour
{
    private Image introHolder;
    private bool isIntro = true;

    private void Awake()
    {
        introHolder = GetComponent<Image>();
    }
    private void Start()
    {
        AudioManager.instance.PlaySound("background1");
        //Cursor.visible = false;
    }
    private void Update()
    {
        if (!isIntro) { return; }
        if(Input.GetKeyDown(KeyCode.Space) && GameManager.instance.canPlay == false)
        {
            //introHolder.DOFade(0f, 1f).onComplete = DisableThis;
            introHolder.GetComponent<CanvasGroup>().DOFade(0f, 1f).onComplete = DisableThis;
            GameManager.instance.StartMugFunction();
        }
    }

    void DisableThis()
    {
        gameObject.SetActive(false);
    }
}
