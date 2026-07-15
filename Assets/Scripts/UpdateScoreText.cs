
using TMPro;
using UnityEngine;

public class UpdateScoreText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI highScoreTxt;
    [SerializeField] private TextMeshProUGUI deliveredCargoTxt;

    void Start()
    {
        // Обращаемся к статическим переменным класса Timer напрямую
        int finalScore = Timer.finScore;
        int finalHighScore = Timer.highScoreFin;
        int finalCargoDelivered = Timer.cargoDeliveredFin;

        // Обновляем текст один раз при старте финальной сцены
        if (scoreTxt != null)
            scoreTxt.text = "Score: " + finalScore;

        if (highScoreTxt != null)
            highScoreTxt.text = "Highscore: " + finalHighScore;

        if (deliveredCargoTxt != null)
            deliveredCargoTxt.text = "Delivered Cargo: " + finalCargoDelivered;
    }
}