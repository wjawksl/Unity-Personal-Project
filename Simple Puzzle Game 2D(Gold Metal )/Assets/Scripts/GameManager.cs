using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("------------[ Core ]")]
    public int animMaxLevel;
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("------------[ Object Pooling ]")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;

    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Header("------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
       
    public enum Sfx {LevelUp, Next, Attach, Button, Over };
    int sfxCursor;
    float deltaTime;
    DateTime dt;

    [Header("------------[ UI ]")]
    public GameObject startGroup;
    public GameObject middleGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    [Header("------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    [Header("------------[ TimeAttackMode ]")]
    public bool isTimeAttack;
    public float curTime;
    public float timeStamp;
    public float timeOverStamp;
    public float limit;
    public float lineLimit;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        lineLimit = line.transform.position.y + 0.5f;
        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }
        if(!PlayerPrefs.HasKey("MaxScore")) // 최고 점수 세팅
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }                 
        if (!PlayerPrefs.HasKey("MaxTime"))  // 최장 시간 세팅
        {
            PlayerPrefs.SetInt("MaxTime", 0);
        }       
    }
    public void GameStart()
    {
        SfxPlay(Sfx.Button);
        startGroup.SetActive(false);
        middleGroup.SetActive(true);        
    }
    public void PlayNormalMode()
    {
        // 모드 선택 UI 비활성화
        middleGroup.SetActive(false);
        SfxPlay(Sfx.Button);

        // 게임 내 요소들 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();

        // 사운드 플레이
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        // 게임 시작
        Invoke("NextDongle", 1.5f);
    }
    public void TimeAttackMode()
    {
        // 타임어택 모드 설정
        SetTimer();
        // 모드 선택 UI 비활성화
        middleGroup.SetActive(false);
        SfxPlay(Sfx.Button);

        // 게임 내 요소들 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        maxScoreText.text = dt.AddSeconds(PlayerPrefs.GetFloat("MaxTime")).ToString("mm:ss");

        // 사운드 플레이
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        // 게임 시작
        Invoke("NextDongle", 1.5f);
    }
    Dongle MakeDongle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);
        // 동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        
        instantDongle.manager = this; // 각 동글이에게 매니저 할당
        instantDongle.effect = instantEffect; // 이펙트 할당        
        donglePool.Add(instantDongle); // 풀링에 추가

        return instantDongle;
    }
    Dongle GetDongle() // 비활성화되어 있는 오브젝트 풀링으로 미리 만들어진 동글이를 반환한다.
    {
        poolCursor = 0;
        for (int i = 0; i < donglePool.Count; i++)
        {           
            if(!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
            poolCursor = (poolCursor + 1) % donglePool.Count;
        }
        return MakeDongle(); // 비활성화된 동글이가 없다면 새로 만든다.
    }
    void NextDongle() // 동글이를 화면에 표시
    {
        if(isOver)
        {
            return;
        }       
        lastDongle = GetDongle(); // 동글이를 가져온다.
        lastDongle.level = Random.Range(0, maxLevel - 1);       
        lastDongle.gameObject.SetActive(true);

        SfxPlay(GameManager.Sfx.Next);
        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext() // 일정 시간 후 동글이 생성
    {
        while(lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);       
        NextDongle();
    }
    public void TouchDown() // 마우스로 동글이를 드래그하는 함수
    {
        if (lastDongle == null)
            return;
        lastDongle.Drag();
    }
    public void TouchUp() // 마우스로 동글이를 떨어뜨리는 함수
    {
        if (lastDongle == null)
            return;
        lastDongle.Drop();
        lastDongle = null;
    }
    public void GameOver() // 게임 종료
    {
        if(isOver)
        {
            return;
        }
        isOver = true;
        timeOverStamp = curTime;
        StartCoroutine(GameOverRoutine());
    }
    IEnumerator GameOverRoutine()
    {       
        // 1. 장면 안에 활성화 되어있는 모든 동글 가져오기
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        // 2. 지우기 전에 모든 동글의 물리효과 비활성화
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false;
        }
        // 3. 1번의 목록을 하나씩 접근해서 지우기
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1.0f);

        int maxScore;

        if (isTimeAttack)
        {
            // 최고 시간 갱신
            float maxTime = Mathf.Max(PlayerPrefs.GetFloat("MaxTime"), curTime);
            PlayerPrefs.SetFloat("MaxTime", maxTime);

            // 게임오버 UI 표시
            subScoreText.text = "기록: " + scoreText.text;
            endGroup.SetActive(true);
        }
        else
        {
            // 최고 점수 갱신
            maxScore = Mathf.Max(PlayerPrefs.GetInt("MaxScore"), score);
            PlayerPrefs.SetInt("MaxScore", maxScore);

            // 게임오버 UI 표시
            subScoreText.text = "점수: " + scoreText.text;
            endGroup.SetActive(true);
        }        
        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }
    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine(ResetCoroutine());
    }
    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Main");
    }
    public void SfxPlay(Sfx type) // 소리 재생 함수
    {
        switch(type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
    void SetTimer() // 타이머 세팅
    {
        isTimeAttack = true;        
        timeStamp = Time.time;
    }
    public IEnumerator updateLine(Vector3 Pos, float val) // 라인 위치 컨트롤 함수
    {
        int frameCount = 0;
        float yPos = Math.Min(line.transform.position.y + val, lineLimit);
        Vector3 nextLinePos = new Vector3(line.transform.position.x, yPos, 0);

        while (frameCount <= 20)
        {
            frameCount++;
            line.transform.position = Vector3.Lerp(line.transform.position, nextLinePos, 0.3f);
            yield return null;           
        }       
    }
    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }                  
    }
    private void LateUpdate()
    {     
        if (isTimeAttack) // 타임어택 모드라면
        {
            curTime = Time.time - timeStamp;
            if (isOver)
            {
                scoreText.text = dt.AddSeconds(timeOverStamp).ToString("mm:ss");
                GameOver();
            }
            else
            {
                scoreText.text = dt.AddSeconds(curTime).ToString("mm:ss"); // 진행 시간 표시
                deltaTime += Time.deltaTime;
                if(deltaTime >= limit)
                {
                    deltaTime = 0;                   
                    StartCoroutine(updateLine(line.transform.position, -0.5f)); // 일정 시간마다 라인 위치 컨트롤                  
                }                
            } 
        }
        else scoreText.text = score.ToString();
    }
}
