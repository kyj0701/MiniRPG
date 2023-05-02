using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    SpriteRenderer player;
    Vector3 rightPos = new Vector3(0.8f, 1.0f, 0);
    Vector3 leftPos = new Vector3(-0.8f, 1.0f, 0);

    private void Awake()
    {
        player = GetComponentsInParent<SpriteRenderer>()[0];
    }

    private void LateUpdate()
    {
        bool isReverse = player.flipX;

        transform.localPosition = isReverse ? leftPos : rightPos;
    }
}
