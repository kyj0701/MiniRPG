using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : Enemy
{
    public Transform wallCheck;

    private void Awake()
    {
        base.Awake();
        speed = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isHit)
        {
            rigid.velocity = new Vector2(-transform.localScale.x * speed, rigid.velocity.y);

            if (Physics2D.OverlapCircle(wallCheck.position, 0.01f, layer) || Physics2D.OverlapCircle(wallCheck.position, 0.01f, GameManager.instance.playerLayer))
            {
                MonsterFlip();
            }
        }

    }
}