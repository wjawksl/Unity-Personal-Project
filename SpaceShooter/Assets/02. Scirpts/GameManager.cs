using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();
    public List<GameObject> monsterPool = new List<GameObject>();

    public int maxMonster = 10;
    public GameObject monster;
    public float createTime = 3.0f;
    private bool isGameOver;
    public bool IsGameOver
    {
        get{ return isGameOver; }
        set{
            isGameOver = value;
            if(isGameOver)
            {
                CancelInvoke("CreateMonster");
            }
        }
    }
    public static GameManager instance = null;

    public TMP_Text scoreText;
    private int totScore = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        CreateMosterPool();
        Transform spawnPointGroup = GameObject.Find("SpawnPointGroup")?.transform;
        //spawnPointGroup?.GetComponentsInChildren<Transform>(points);
        foreach(Transform point in spawnPointGroup)
        {
            points.Add(point);
        }
        InvokeRepeating("CreateMonster", 2.0f, createTime);
        totScore = PlayerPrefs.GetInt("TOT_SCORE", 0);
        DisplayScore(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CreateMonster()
    {
        int idx = Random.Range(0, points.Count);
        //Instantiate(monster, points[idx].position, points[idx].rotation);

        GameObject _monster = GetMosterInPool();
        _monster?.transform.SetPositionAndRotation(points[idx].position, points[idx].rotation);
        _monster?.SetActive(true);
    }
    void CreateMosterPool()
    {
        for(int i = 0; i < maxMonster; i++)
        {
            var _monster = Instantiate<GameObject>(monster);

            _monster.name = $"Monster_{i:00}";
            _monster.SetActive(false);
            monsterPool.Add(_monster);
        }
    }
    public GameObject GetMosterInPool()
    {
        foreach(var _monster in monsterPool)
        {
            if(_monster.activeSelf == false)
            {
                return _monster;
            }
        }
        return null;
    }
    public void DisplayScore(int score)
    {
        totScore += score;
        scoreText.text = $"<color=#00ff00>SCORE:</color> <color=#ff0000>{totScore: #,##0}</color>";
        PlayerPrefs.SetInt("TOT_SCORE", totScore);
    }
}
