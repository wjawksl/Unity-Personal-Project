using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public float maxSpeed;
    public float jumpPower;
    public Animator anim;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;    
    CapsuleCollider2D capsuleCollider = new CapsuleCollider2D();
    AudioSource audioSource;

    // Start is called before the first frame update
    void Awake()
    {       
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(string action)
    {
        switch(action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }
        audioSource.Play();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(0.5f * rigid.velocity.normalized.x, rigid.velocity.y);
        }        
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        } 
        if (Mathf.Abs(rigid.velocity.x) < 0.3f) anim.SetBool("isWalking", false);
        else anim.SetBool("isWalking", true);        
    }
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < -maxSpeed)
            rigid.velocity = new Vector2(-maxSpeed, rigid.velocity.y);

        if (rigid.velocity.y < 0)
        {
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector2.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.8f)
                {
                    anim.SetBool("isJumping", false);
                }
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Attack
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);                
            }
            else // Damaged
            {
                OnDamaged(collision.transform.position);                
            }
                
        }
    }
    void OnDamaged(Vector2 targetPos)
    {
        gameManager.HealthDown();
        gameObject.layer = 9;

        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        int dirc_X = transform.position.x - targetPos.x > 0 ? 1 : -1;
        
        rigid.AddForce(new Vector2(dirc_X, 1) * 7, ForceMode2D.Impulse);

        anim.SetTrigger("damaged");
        // Play Sound
        PlaySound("DAMAGED");
        Invoke("OffDamaged", 2.0f);      
    }
    void OffDamaged()
    {
        gameObject.layer = 8;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }
    void OnAttack(Transform enemy)
    {
        // Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
        // Play Sound
        PlaySound("ATTACK");
        // Get Point
        gameManager.stagePoint += 100;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            // Get Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSliver = collision.gameObject.name.Contains("Sliver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSliver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;
            // Play Sound
            PlaySound("ITEM");

            // Deactive Item
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            // Play Sound
            PlaySound("FINISH");
            // Next Stage
            gameManager.NextStage();
        }
    }
    public void OnDie()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // Sprite Flip
        spriteRenderer.flipY = true;
        // Collider Disable
        capsuleCollider.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // Play Sound
        PlaySound("DIE");
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector3.zero;
    }
}
