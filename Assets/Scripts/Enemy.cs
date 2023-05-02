using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int curHp;
    public float speed;
    public float atkCoolTime = 3f;
    public float atkCoolTimeCalc = 3f;

    bool isLive;
    public bool isHit = false;
    public bool isGround = true;
    public bool canAtk = true;
    public bool isRight;

    protected Rigidbody2D rigid;
    protected BoxCollider2D box;
    public GameObject hitBox;
    public Animator anim;
    public LayerMask layer;

    protected void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();

        StartCoroutine(CalcCoolTime());
        StartCoroutine(ResetCollider());
    }

    IEnumerator ResetCollider()
    {
        while (true)
        {
            yield return null;
            if (!hitBox.activeInHierarchy)
            {
                yield return new WaitForSeconds(0.5f);
                hitBox.SetActive(true);
                isHit = false;
            }
        }
    }

    IEnumerator CalcCoolTime()
    {
        while (true)
        {
            yield return null;
            if (!canAtk)
            {
                atkCoolTimeCalc -= Time.deltaTime;
                if (atkCoolTimeCalc <= 0)
                {
                    atkCoolTimeCalc = atkCoolTime;
                    canAtk = true;
                }
            }
        }
    }

    public bool IsPlayingAnim(string name)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
        {
            return true;
        }
        return false;
    }
    public void MyAnimSetTrigger(string name)
    {
        if (!IsPlayingAnim(name))
        {
            anim.SetTrigger(name);
        }
    }

    protected void MonsterFlip()
    {
        isRight = !isRight;

        Vector3 thisScale = transform.localScale;
        if (isRight)
        {
            thisScale.x = -Mathf.Abs(thisScale.x);
        }
        else
        {
            thisScale.x = Mathf.Abs(thisScale.x);
        }
        transform.localScale = thisScale;
        rigid.velocity = Vector2.zero;
    }

    protected bool IsPlayerDir()
    {
        if (transform.position.x < GameManager.instance.player.transform.position.x ? isRight : !isRight)
        {
            return true;
        }
        return false;
    }

    public void TakeDamage(int damage)
    {
        curHp -= damage;
        isHit = true;

        hitBox.SetActive(false);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        //if ( collision.transform.CompareTag ( ?? ) )
        //{
        //TakeDamage ( 0 );
        //}
    }
}
