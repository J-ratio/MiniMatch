using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Button restartButton;

    private int score = 0;

    void Start()
    {
        UpdateScore(0);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    public void UpdateScore(int newScore)
    {
        score = newScore;
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void IncrementScore()
    {
        UpdateScore(score + 1);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
