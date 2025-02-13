using UnityEngine;

public class Foam : MonoBehaviour
{
    [HideInInspector] public Transform topOfLiquid;
    [HideInInspector] private SpriteRenderer spriteRen;
    [HideInInspector] public Sprite foam1;

    public void MoveFoam()
    {
        float pos = GameManager.instance.ReturnPosOfTopLiquid();
        transform.position = new Vector2(transform.position.x, pos);
    }

}
