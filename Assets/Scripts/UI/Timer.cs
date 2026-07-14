using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event Action onSecPassed;
    public TextMeshProUGUI timeText;
    public static Timer Instance;
    public CargoSpawner cargoSpawner;
    private int time;
    private int secPassed;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //cargoSpawner = GameObject.FindFirstObjectByType<CargoSpawner>();
        //if (cargoSpawner)
        //    onSecPassed += cargoSpawner.SpawnRandomInActive;

        time = 120;
        secPassed = 0;
        UpdateTimer();
    }
    //public void Passed()
    //{

    //}
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
            time--; secPassed++;
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            timeText.text = timeSpan.ToString(@"mm\:ss");
        }
        else
        {
            WarningText taskText = GameObject.FindFirstObjectByType<WarningText>();
            taskText.TimesUpMessage();
        }
        if (time > 0 && secPassed >= 0 && secPassed % 3 == 0)
        {
            onSecPassed?.Invoke();
            //Debug.Log("Sec passed, cargo spawn event fired");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
