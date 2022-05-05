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
        if(!PlayerPrefs.HasKey("MaxScore")) // �ְ� ���� ����
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }                 
        if (!PlayerPrefs.HasKey("MaxTime"))  // ���� �ð� ����
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
        // ��� ���� UI ��Ȱ��ȭ
        middleGroup.SetActive(false);
        SfxPlay(Sfx.Button);

        // ���� �� ��ҵ� Ȱ��ȭ
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();

        // ���� �÷���
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        // ���� ����
        Invoke("NextDongle", 1.5f);
    }
    public void TimeAttackMode()
    {
        // Ÿ�Ӿ��� ��� ����
        SetTimer();
        // ��� ���� UI ��Ȱ��ȭ
        middleGroup.SetActive(false);
        SfxPlay(Sfx.Button);

        // ���� �� ��ҵ� Ȱ��ȭ
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        maxScoreText.text = dt.AddSeconds(PlayerPrefs.GetFloat("MaxTime")).ToString("mm:ss");

        // ���� �÷���
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        // ���� ����
        Invoke("NextDongle", 1.5f);
    }
    Dongle MakeDongle()
    {
        // ����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);
        // ���� ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        
        instantDongle.manager = this; // �� �����̿��� �Ŵ��� �Ҵ�
        instantDongle.effect = instantEffect; // ����Ʈ �Ҵ�        
        donglePool.Add(instantDongle); // Ǯ���� �߰�

        return instantDongle;
    }
    Dongle GetDongle() // ��Ȱ��ȭ�Ǿ� �ִ� ������Ʈ Ǯ������ �̸� ������� �����̸� ��ȯ�Ѵ�.
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
        return MakeDongle(); // ��Ȱ��ȭ�� �����̰� ���ٸ� ���� �����.
    }
    void NextDongle() // �����̸� ȭ�鿡 ǥ��
    {
        if(isOver)
        {
            return;
        }       
        lastDongle = GetDongle(); // �����̸� �����´�.
        lastDongle.level = Random.Range(0, maxLevel - 1);       
        lastDongle.gameObject.SetActive(true);

        SfxPlay(GameManager.Sfx.Next);
        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext() // ���� �ð� �� ������ ����
    {
        while(lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);       
        NextDongle();
    }
    public void TouchDown() // ���콺�� �����̸� �巡���ϴ� �Լ�
    {
        if (lastDongle == null)
            return;
        lastDongle.Drag();
    }
    public void TouchUp() // ���콺�� �����̸� ����߸��� �Լ�
    {
        if (lastDongle == null)
            return;
        lastDongle.Drop();
        lastDongle = null;
    }
    public void GameOver() // ���� ����
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
        // 1. ��� �ȿ� Ȱ��ȭ �Ǿ��ִ� ��� ���� ��������
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        // 2. ����� ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false;
        }
        // 3. 1���� ����� �ϳ��� �����ؼ� �����
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1.0f);

        int maxScore;

        if (isTimeAttack)
        {
            // �ְ� �ð� ����
            float maxTime = Mathf.Max(PlayerPrefs.GetFloat("MaxTime"), curTime);
            PlayerPrefs.SetFloat("MaxTime", maxTime);

            // ���ӿ��� UI ǥ��
            subScoreText.text = "���: " + scoreText.text;
            endGroup.SetActive(true);
        }
        else
        {
            // �ְ� ���� ����
            maxScore = Mathf.Max(PlayerPrefs.GetInt("MaxScore"), score);
            PlayerPrefs.SetInt("MaxScore", maxScore);

            // ���ӿ��� UI ǥ��
            subScoreText.text = "����: " + scoreText.text;
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
    public void SfxPlay(Sfx type) // �Ҹ� ��� �Լ�
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
    void SetTimer() // Ÿ�̸� ����
    {
        isTimeAttack = true;        
        timeStamp = Time.time;
    }
    public IEnumerator updateLine(Vector3 Pos, float val) // ���� ��ġ ��Ʈ�� �Լ�
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
        if (isTimeAttack) // Ÿ�Ӿ��� �����
        {
            curTime = Time.time - timeStamp;
            if (isOver)
            {
                scoreText.text = dt.AddSeconds(timeOverStamp).ToString("mm:ss");
                GameOver();
            }
            else
            {
                scoreText.text = dt.AddSeconds(curTime).ToString("mm:ss"); // ���� �ð� ǥ��
                deltaTime += Time.deltaTime;
                if(deltaTime >= limit)
                {
                    deltaTime = 0;                   
                    StartCoroutine(updateLine(line.transform.position, -0.5f)); // ���� �ð����� ���� ��ġ ��Ʈ��                  
                }                
            } 
        }
        else scoreText.text = score.ToString();
    }
}
