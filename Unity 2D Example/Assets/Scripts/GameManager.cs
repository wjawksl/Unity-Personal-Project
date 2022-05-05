using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex = 0;
    public int health = 3;
    public PlayerMove player;
    public GameObject[] stages;
    private float[] yPos = { 1.5f, -29.5f};

    public Image[] UIHealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;
    void Start()
    {
        transform.position = new Vector2(0, yPos[0]);
    }
    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();        
    }
    public void NextStage()
    {
        // Change Stage
        if(stageIndex < stages.Length - 1)
        {
            stages[stageIndex].SetActive(false);
            stageIndex++;
            stages[stageIndex].SetActive(true);
            transform.position = new Vector2(0, yPos[stageIndex]);
            PlayerReposition();

            UIStage.text = $"STAGE {stageIndex + 1}";
        }
        else
        {
            // Game Clear

            //Player Control Lock
            Time.timeScale = 0;

            Debug.Log("Game Clear!!");            
            Text btnText = UIRestartBtn.GetComponentInChildren<Text>();
            btnText.text = "Clear!!";
            UIRestartBtn.SetActive(true);
        }
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        health--;
        UIHealth[health].color = new Color(1, 0, 0, 0.4f);
        if (health == 0)
        {
            // Player Die Effect
            player.OnDie();

            //Result UI
            Debug.Log("DIE!!");
            // Retry Button UI
            UIRestartBtn.SetActive(true);
        }                 
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            // Move Player Back
            if(health > 1)
                PlayerReposition();
            //Health Down
            HealthDown();
        }
    }
    void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 0, 0);
        player.anim.SetBool("isJumping", false);                
        player.VelocityZero();
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
