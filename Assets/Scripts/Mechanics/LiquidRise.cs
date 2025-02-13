using NUnit.Framework.Constraints;
using UnityEngine;

public class LiquidRise : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer spriteRen;
    private Jar jar;

    private void Awake()
    {
        spriteRen = GetComponent<SpriteRenderer>();
        jar = GetComponentInParent<Jar>();
    }
    public void RiseTheLiquid(float amount)
    {
        //Vector2 newScale = new Vector2(transform.localScale.x, transform.localScale.y + amount);
        //transform.localScale = newScale;

        Vector2 newScale = new Vector2(spriteRen.size.x, spriteRen.size.y + amount);
        spriteRen.size = newScale;
    }
    public Vector2 ReturnGoalPosition()
    {
        return jar.ReturnGoalPosition();
    }
    public float ReturnMaxJarHeight()
    {
        return jar.maxHeight;
    }
    public float ReturnSizeY()
    {
        return spriteRen.size.y;
    }
}
