using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    [SerializeField] float spped = 4.0f;
    [SerializeField] float jumpPower = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;

    public int curHp = 20;
    public Vector2 boxSize;
    public GameObject hitBox;
    public BoxCollider2D weaponColl;

    private Animator anim;
    private Rigidbody2D rigid;
    private Sensor m_groundSensor;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private bool isHit = false;
    private bool invincible = false;


    // Use this for initialization
    void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        m_groundSensor = GetComponentsInChildren<Sensor>()[0];

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

    // Update is called once per frame
    void Update()
    {
        if (IsPlayingAnim("Hurt") || isHit) return;

        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // Disable rolling if timer extends duration
        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            anim.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            anim.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }

        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling)
        {
            rigid.velocity = new Vector2(inputX * spped, rigid.velocity.y);
        }

        //Set AirSpeed in animator
        anim.SetFloat("AirSpeedY", rigid.velocity.y);

        // -- Handle Animations --
        //Death
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            anim.SetBool("noBlood", m_noBlood);
            anim.SetTrigger("Death");
        }
        //Hurt
        else if (Input.GetKeyDown("q") && !m_rolling) anim.SetTrigger("Hurt");
        //Attack
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            anim.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            anim.SetTrigger("Block");
            anim.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
            anim.SetBool("IdleBlock", false);

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling)
        {
            m_rolling = true;
            anim.SetTrigger("Roll");
            rigid.velocity = new Vector2(m_facingDirection * m_rollForce, rigid.velocity.y);
        }


        //Jump
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        {
            anim.SetTrigger("Jump");
            m_grounded = false;
            anim.SetBool("Grounded", m_grounded);
            rigid.velocity = new Vector2(rigid.velocity.x, jumpPower);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            anim.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                anim.SetInteger("AnimState", 0);
        }

        if (!IsPlayingAnim("Hurt")) isHit = false;
        if (!IsPlayingAnim("Attack" + m_currentAttack)) weaponColl.enabled = false;
    }

    public bool IsPlayingAnim(string name)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
        {
            return true;
        }
        return false;
    }

    public void WeaponColliderOnOff()
    {
        //if (!weaponColl.enabled && IsPlayingAnim("Attack" + m_currentAttack))
        //{
        //    switch(m_currentAttack)
        //    {
        //        case 1:
        //            weaponColl.size = new Vector2(1.2f, 1.3f);
        //            break;
        //        case 2:
        //            break;
        //        case 3:
        //            break;
        //    }
        //}
        weaponColl.enabled = !weaponColl.enabled;
    }


    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            if (!invincible)
            {
                StartCoroutine(InvincibleEffect());
                Debug.Log("Player Hit");
            }

            rigid.velocity = Vector2.zero;
            isHit = true;
            Invoke("IsHitResult", 0.5f);
            hitBox.SetActive(false);

            if (transform.position.x > collision.transform.position.x)
            {
                rigid.velocity = new Vector2(5f, 1.5f);
            }
            else
            {
                rigid.velocity = new Vector2(-5f, 1.5f);
            }

            if (!IsPlayingAnim("Attack" + m_currentAttack)) anim.SetTrigger("Hurt");
        }
    }

    private void IsHitResult()
    {
        isHit = false;
    }

    IEnumerator InvincibleEffect()
    {
        invincible = true;

        anim.SetTrigger("Hurt");

        yield return new WaitForSeconds(1f);

        invincible = false;

        anim.SetInteger("AnimState", 0);
    }
}
