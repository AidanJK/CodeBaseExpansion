using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public static ColorManager instance;
    [Header("SIPHON AND COLOR VARIATIONS")]
    public Image siphon;
    public Color firstColor;
    public Color colorBad;
    public Color colorGood;
    public Color colorPerfect;

    private void Awake()
    {
        instance = this;
        siphon.color = firstColor;
    }

    public Image ReturnSiphonImage()
    {
        return siphon;
    }

}
