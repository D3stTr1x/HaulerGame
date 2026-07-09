using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public static Timer Instance;
    private int time;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        time = 120;
        UpdateTimer();
    }
    void UpdateTimer()
    {
        InvokeRepeating("UpdateTimeDisplay", 0, 1);
    }
    public void AddTime(int s)
    {
        time += s; //maybe not needed
    }
    public void AddTime()
    {
        time += 30; //maybe not needed
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
            WarningText taskText = GameObject.FindFirstObjectByType<WarningText>();
            taskText.TimesUpMessage();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
