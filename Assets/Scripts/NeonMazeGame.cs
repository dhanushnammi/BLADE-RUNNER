using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NeonMazeGame : MonoBehaviour
{
    public static NeonMazeGame Instance;
    public Text scoreText, timerText, messageText;
    public GameObject endPanel;
    public Text endTitle, endDetails;
    public int totalCoins = 0;
    public int score = 0;
    public float timeLimit = 180f;
    float timeLeft;
    bool ended;

    void Awake() { Instance = this; timeLeft = timeLimit; }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (ended) return;
        timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = "TIME  " + Mathf.CeilToInt(Mathf.Max(0,timeLeft)).ToString();
        if (timeLeft <= 0) Finish(false, "TIME'S UP!");
    }
    public void SetTotalCoins(int n) { totalCoins = n; Refresh(); }
    public void AddCoin()
    {
        if (ended) return;
        score += 10; Refresh();
        if (score / 10 >= totalCoins) Finish(true, "MAZE CLEARED!");
    }
    void Refresh()
    {
        if (scoreText) scoreText.text = "CORES  " + (score/10) + " / " + totalCoins;
    }
    public void Finish(bool won, string title)
    {
        if (ended) return;
        ended = true;
        if (endPanel) endPanel.SetActive(true);
        if (endTitle) endTitle.text = title;
        if (endDetails) endDetails.text = won ? "All energy cores collected.\nPress R to run again." : "You ran out of time.\nPress R to try again.";
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var p = FindObjectOfType<NeonPlayer>();
        if (p) p.enabled = false;
    }
    public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
}
