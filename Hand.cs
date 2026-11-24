using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;
    public SpriteRenderer sr;
    
    SpriteRenderer player;
    
    Vector3 rightPos = new Vector3(0.35f, -0.15f, 0f);
    Vector3 rightPosReverse = new Vector3(-0.35f, -0.15f, 0f);
    Quaternion leftRot = Quaternion.Euler(0f, 0f, -35);
    Quaternion leftRotReverse = Quaternion.Euler(0f, 0f, -135);
    private void Awake()
    {
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }


    private void LateUpdate()
    {
        bool isReverse = player.flipX;

        if (isLeft)
        {
            transform.localRotation = isReverse ? leftRotReverse : leftRot;
            sr.flipY = isReverse;
            sr.sortingOrder = isReverse ? 4 : 6;
        }
        else
        {
            transform.localPosition = isReverse ? rightPosReverse : rightPos;
            sr.flipX = isReverse;
            sr.sortingOrder = isReverse ? 6 : 4;
        }
    }
}
