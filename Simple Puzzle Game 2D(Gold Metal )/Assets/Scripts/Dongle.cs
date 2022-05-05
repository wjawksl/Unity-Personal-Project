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
    private void OnEnable() // 오브젝트 활성화시 호출
    {
        anim.SetInteger("Level", level);
    }
    private void OnDisable() // 오브젝트 비활성화시 호출
    {
        // 동글 속성 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;
        // 동글 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        //동글 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circleColl.enabled = true;
    }
    void Update() // 마우스를 따라가는 동글이 위치를 업데이트
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
        StartCoroutine(AttachRoutine()); // 충돌 시 충돌 사운드 재생 함수
    }
    IEnumerator AttachRoutine()
    {
        if (isAttach) yield break; // 이미 붙어있다면 탈출

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
                // 1. 내가 아래에 있을 때
                // 2. 동일한 높이일 때, 내가 오른쪽에 있을 때
                if (meY < otherY || (meY == otherY && meX > otherX))
                {
                    // 상대방은 숨기기
                    other.Hide(transform.position); // 충돌대상을 숨긴다.
                    // 나는 레벨업                   
                    if (level == manager.animMaxLevel) // 애니메이션 실행 가능한 최대레벨이라면
                    {
                        Hide(Vector3.up * 100); // 본인도 숨긴다.
                    }
                    else LevelUp(); // 레벨업한다.
                }
            }
        }
    }
    public void Hide(Vector3 targetPos) // 숨김 함수
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
            if (targetPos != Vector3.up * 100) // 존재 가능한 위치라면
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f); // 동글이를 합친다
            }
            else if (targetPos == Vector3.up * 100) // 존재 가능한 위치가 아니라면
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f); // 동글이의 크기를 0으로 만듦
            }
            yield return null;
        }
        if (targetPos != Vector3.up * 100 || manager.isOver) // 존재 가능한 위치 or 게임이 끝났다면
        {
            if(!manager.isTimeAttack)
                manager.score += (int)Mathf.Pow(2, level); // 타임어택 모드가 아니라면 점수 갱신

            if(manager.isTimeAttack && !manager.isOver) // 타임어택 모드이면서 게임이 진행중이라면
            {
                StartCoroutine(manager.updateLine(manager.line.transform.position, 0.1f + level * 0.4f)); // 점수를 얻는대신 라인의 위치를 증가시킴
            }
        }
        else // 게임이 진행중이며 존재 가능한 위치가 아니라면(본인도 숨겨야 한다면)
        {
            manager.SfxPlay(GameManager.Sfx.LevelUp); // 레벨 업 함수가 실행되지 않기 때문에 소리와 이펙트를 출력
            EffectPlay();
        }
        isMerge = false;
        gameObject.SetActive(false); // 오브젝트 비활성화
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

        anim.SetInteger("Level", level + 1); // 연속으로 합쳐질 경우 일정 대기시간을 주기 위해 level + 1을 사용
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);
        isMerge = false;
    }
    void OnTriggerStay2D(Collider2D collision) // 선에 닿고 있을 때 처리하는 함수
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
    void OnTriggerExit2D(Collider2D collision) // 선에서 빠져나갔을 때 처리하는 함수
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
