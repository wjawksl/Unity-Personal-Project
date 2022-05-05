using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    private Rigidbody2D rigid;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    public int nextMove;
    private float nextThinkTime;
    CapsuleCollider2D capsuleCollider = new CapsuleCollider2D();
    
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        nextThinkTime = Random.Range(3.0f, 5.0f);
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        Invoke("Think", nextThinkTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        //Check Platform
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.2f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector2.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, 1, LayerMask.GetMask("Platform"));

        // Return
        if (rayHit.collider == null) Return();       
    }
    void Think()
    {
        // Set Next Active
        nextMove = Random.Range(-1, 2);
        nextThinkTime = Random.Range(3.0f, 5.0f);
        // Set Animation
        anim.SetInteger("walkSpeed", nextMove);

        // Set Flip Sprite
        if (nextMove != 0)
            spriteRenderer.flipX = nextMove == 1;  
        
        // Recursive
        Invoke("Think", nextThinkTime);
    }
    void Return()
    {
        nextMove *= -1;
        if (nextMove != 0)
            spriteRenderer.flipX = nextMove == 1;
        /*CancelInvoke();
        Invoke("Think", nextThinkTime);*/
    }
    public void OnDamaged()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // Sprite Flip
        spriteRenderer.flipY = true;
        // Collider Disable
        capsuleCollider.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // Destroy
        Invoke("DeActive", 5.0f);
    }
    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
