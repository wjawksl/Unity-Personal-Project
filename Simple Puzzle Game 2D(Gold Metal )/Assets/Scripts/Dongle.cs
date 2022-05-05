using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{  
    public int level;
    public bool isDrag;
    public bool isMerge;
    public Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CircleCollider2D circleColl;
    public GameManager manager;
    public ParticleSystem effect;
    public bool isAttach;

    float deadTime;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circleColl = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnEnable() // ������Ʈ Ȱ��ȭ�� ȣ��
    {
        anim.SetInteger("Level", level);
    }
    private void OnDisable() // ������Ʈ ��Ȱ��ȭ�� ȣ��
    {
        // ���� �Ӽ� �ʱ�ȭ
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;
        // ���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        //���� ���� �ʱ�ȭ
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circleColl.enabled = true;
    }
    void Update() // ���콺�� ���󰡴� ������ ��ġ�� ������Ʈ
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder = -3.93f + transform.localScale.x / 2f;
            float rightBorder = 3.93f - transform.localScale.x / 2f;
            mousePos.y = 8;
            mousePos.z = 0;
            if (mousePos.x < leftBorder) mousePos.x = leftBorder;
            else if (mousePos.x > rightBorder) mousePos.x = rightBorder;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
    }
    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine()); // �浹 �� �浹 ���� ��� �Լ�
    }
    IEnumerator AttachRoutine()
    {
        if (isAttach) yield break; // �̹� �پ��ִٸ� Ż��

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);
        yield return new WaitForSeconds(0.2f);
        isAttach = false;
    }
    void OnCollisionStay2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // Get position
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                // 1. ���� �Ʒ��� ���� ��
                // 2. ������ ������ ��, ���� �����ʿ� ���� ��
                if (meY < otherY || (meY == otherY && meX > otherX))
                {
                    // ������ �����
                    other.Hide(transform.position); // �浹����� �����.
                    // ���� ������                   
                    if (level == manager.animMaxLevel) // �ִϸ��̼� ���� ������ �ִ뷹���̶��
                    {
                        Hide(Vector3.up * 100); // ���ε� �����.
                    }
                    else LevelUp(); // �������Ѵ�.
                }
            }
        }
    }
    public void Hide(Vector3 targetPos) // ���� �Լ�
    {
        isMerge = true;
        rigid.simulated = false;
        circleColl.enabled = false;

        if (targetPos == Vector3.up * 100 && manager.isOver)
        {
            EffectPlay();
        }
        StartCoroutine(HideRoutine(targetPos));
    }
    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;
        while (frameCount < 20)
        {
            frameCount++;
            if (targetPos != Vector3.up * 100) // ���� ������ ��ġ���
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f); // �����̸� ��ģ��
            }
            else if (targetPos == Vector3.up * 100) // ���� ������ ��ġ�� �ƴ϶��
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f); // �������� ũ�⸦ 0���� ����
            }
            yield return null;
        }
        if (targetPos != Vector3.up * 100 || manager.isOver) // ���� ������ ��ġ or ������ �����ٸ�
        {
            if(!manager.isTimeAttack)
                manager.score += (int)Mathf.Pow(2, level); // Ÿ�Ӿ��� ��尡 �ƴ϶�� ���� ����

            if(manager.isTimeAttack && !manager.isOver) // Ÿ�Ӿ��� ����̸鼭 ������ �������̶��
            {
                StartCoroutine(manager.updateLine(manager.line.transform.position, 0.1f + level * 0.4f)); // ������ ��´�� ������ ��ġ�� ������Ŵ
            }
        }
        else // ������ �������̸� ���� ������ ��ġ�� �ƴ϶��(���ε� ���ܾ� �Ѵٸ�)
        {
            manager.SfxPlay(GameManager.Sfx.LevelUp); // ���� �� �Լ��� ������� �ʱ� ������ �Ҹ��� ����Ʈ�� ���
            EffectPlay();
        }
        isMerge = false;
        gameObject.SetActive(false); // ������Ʈ ��Ȱ��ȭ
    }
    void LevelUp()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1); // �������� ������ ��� ���� ���ð��� �ֱ� ���� level + 1�� ���
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);
        isMerge = false;
    }
    void OnTriggerStay2D(Collider2D collision) // ���� ��� ���� �� ó���ϴ� �Լ�
    {
        if (collision.tag == "Finish") 
        {
            deadTime += Time.deltaTime;

            if (deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if (deadTime > 5)
            {
                manager.GameOver();
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision) // ������ ���������� �� ó���ϴ� �Լ�
    {
        if (collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }
    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;

        effect.Play();
    }
}
