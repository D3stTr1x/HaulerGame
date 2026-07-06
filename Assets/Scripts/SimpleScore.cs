using TMPro;
using UnityEngine;

public class SimpleScore : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public static SimpleScore Instance;

    public int score;
    private int highScore;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateScore(int sc)
    {
        score += sc;
        if (score > highScore)
            highScore = score;
        Debug.Log($"cur score: {score}");
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}\nHighscore: {highScore}";
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
            Instance = new SimpleScore();
        score = 0;
        highScore = score;

        UpdateScoreDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
