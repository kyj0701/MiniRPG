using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    public Transform pos;
    public Vector2 boxSize;

    private Animator anim;
    private Rigidbody2D rigid;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;


    // Use this for initialization
    void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        m_groundSensor = GetComponentsInChildren<Sensor_HeroKnight>()[0];
        //m_wallSensorR1 = GetComponentsInChildren<Sensor_HeroKnight>()[1];
        //m_wallSensorR2 = GetComponentsInChildren<Sensor_HeroKnight>()[2];
        //m_wallSensorL1 = GetComponentsInChildren<Sensor_HeroKnight>()[3];
        //m_wallSensorL2 = GetComponentsInChildren<Sensor_HeroKnight>()[4];
    }

    // Update is called once per frame
    void Update()
    {
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
            rigid.velocity = new Vector2(inputX * m_speed, rigid.velocity.y);

        //Set AirSpeed in animator
        anim.SetFloat("AirSpeedY", rigid.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        //m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        //anim.SetBool("WallSlide", m_isWallSliding);

        //Death
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            anim.SetBool("noBlood", m_noBlood);
            anim.SetTrigger("Death");
        }

        //Hurt
        else if (Input.GetKeyDown("q") && !m_rolling)
            anim.SetTrigger("Hurt");

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

            Collider2D[] colls = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
            foreach (Collider2D coll in colls)
            {
                if (coll.tag == "Enemy") Debug.Log(coll.tag);
            }

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
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
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
            rigid.velocity = new Vector2(rigid.velocity.x, m_jumpForce);
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
    }

    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }
}
