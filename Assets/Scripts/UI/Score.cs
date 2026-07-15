using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public static Score Instance;

    public int score;
    public int highScore;
    public int cargoDelivered;


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
        //Debug.Log($"cur score: {score}");
        UpdateScoreDisplay();
    }
    public void UpdateCargosDelivered() 
    {
        cargoDelivered++;
        UpdateScoreDisplay();
    }
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}\nHighscore: {highScore}\nCargo delivered: {cargoDelivered}";
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
            Instance = new Score();
        score = 0;
        cargoDelivered = 0;
        highScore = score;

        UpdateScoreDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
