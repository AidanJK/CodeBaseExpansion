using UnityEditor;
using UnityEngine;

public class Jar : MonoBehaviour
{
    [Header("JAR VALUES")]
    public string nameOfJar;
    public GameManager.JarType jarType;
    public Transform positionGoal;
    public int points;
    public Sprite sprite;
    public SpriteMask spriteMask;
    [SerializeField] private SpriteRenderer renRef;
    public LiquidRise myLiquid;
    public Foam foam;

    [Header("HANDLE COMPONENTS")]
    [HideInInspector] public Vector2 startPosition;

    [Header("INITIAL VALUES")]
    public Vector2 initalPos;
    [HideInInspector] public float maxHeight;

    private void Awake()
    {
        //GetComponent<SpriteRenderer>().sprite = sprite;
        //GetComponent<SpriteMask>().sprite = spriteMask;
        maxHeight = renRef.size.y;
        gameObject.name = nameOfJar;
        startPosition = myLiquid.gameObject.transform.position;
    }
    public LiquidRise ReturnLiquidRise()
    {
        return myLiquid;
    }
    private void OnEnable()
    {
        foam.gameObject.SetActive(false);
    }
    public Vector2 ReturnGoalPosition()
    {
        return positionGoal.position;
    }
    public void ResetValues()
    {
        myLiquid.spriteRen.size = new Vector2(myLiquid.spriteRen.size.x, 0);
    }

    private void OnDisable()
    {
        //PoolManager.instance.ChangeStartPosition(gameObject);
    }
    public float ReturnYLiquid()
    {
        return myLiquid.transform.position.y;
    }
}
