using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    private int time;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        time = 600;
        UpdateTimer();
    }
    void UpdateTimer()
    {
        InvokeRepeating("UpdateTimeDisplay", 0, 1);
    }
    void UpdateTimeDisplay()
    {
        if (time > 0)
        {
            time--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            timeText.text = timeSpan.ToString(@"mm\:ss");
        }
        else
        {
            timeText.text = "Time's up!";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
