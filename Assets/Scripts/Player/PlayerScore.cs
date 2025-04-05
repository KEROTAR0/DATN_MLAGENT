using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour
{
    public int currentScore = 0;
    public Text scoreText;

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    public void SaveScore() // Gọi khi thoát game hoặc hoàn thành
    {
        PlayerPrefs.SetInt("FinalScore", currentScore);
        PlayerPrefs.Save();
        Debug.Log("Score saved: " + currentScore);
    }

    public static int LoadSavedScore()
    {
        return PlayerPrefs.GetInt("FinalScore", 0);
    }
}
