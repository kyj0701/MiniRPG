using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : Enemy
{

    public enum State
    {
        Idle,
        Run,
        Attack,
    };
    public State curState = State.Idle;

    public Transform wallCheck;

    private void Awake()
    {
        base.Awake();

        StartCoroutine(FSM());
    }

    private void Update()
    {
        if (!isLive) return;

        if (!isHit && !IsPlayingAnim("Run"))
        {
            MyAnimSetTrigger("Idle");
        }
    }

    IEnumerator FSM()
    {
        while (true)
        {
            yield return StartCoroutine(curState.ToString());
        }
    }

    IEnumerator Idle()
    {
        yield return null;
        MyAnimSetTrigger("Idle");

        yield return new WaitForSeconds(1.0f);
        curState = State.Run;
    }

    IEnumerator Run()
    {
        yield return null;
        float runTime = Random.Range(2.0f, 4.0f);

        while (runTime >= 0f)
        {
            runTime -= Time.deltaTime;

            if (!isHit)
            {
                MyAnimSetTrigger("Run");
                rigid.velocity = new Vector2(-transform.localScale.x * speed, rigid.velocity.y);

                if (Physics2D.OverlapCircle(wallCheck.position, 0.01f, layer))
                {
                    EnemyFlip();
                }
                if (canAtk && IsPlayerDir())
                {
                    if (Vector2.Distance(transform.position, GameManager.instance.player.transform.position) < 2.5f)
                    {
                        curState = State.Attack;
                        break;
                    }
                }
            }
            yield return null;
        }
        if (curState != State.Attack)
        {
            if (!IsPlayerDir())
            {
                EnemyFlip();
            }
        }
    }

    IEnumerator Attack()
    {
        yield return null;

        if (!isHit)
        {
            canAtk = false;
            MyAnimSetTrigger("Attack");
            yield return new WaitForSeconds(1.0f);
            curState = State.Idle;
        }
        else
        {
            curState = State.Run;
        }
    }

    public void WeaponColliderOnOff()
    {
        weaponColl.enabled = !weaponColl.enabled;
    }
}
