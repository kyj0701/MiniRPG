using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private SpriteRenderer player;

    Vector3 leftPos = new Vector3(-0.87f, 0.975f, 0f);
    Vector3 rightPos = new Vector3(0.87f, 0.975f, 0f);

    private void Awake()
    {
        player = GetComponentsInParent<SpriteRenderer>()[0];
    }

    private void LateUpdate()
    {
        transform.localPosition = player.flipX ? leftPos : rightPos;
    }
}
