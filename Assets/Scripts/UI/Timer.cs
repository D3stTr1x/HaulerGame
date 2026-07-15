using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event Action onSecPassed;
    public TextMeshProUGUI timeText;
    public static Timer Instance;
    public CargoSpawner cargoSpawner;
    private Score score;
    public static int finScore;
    public static int highScoreFin;
    public static int cargoDeliveredFin;


    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject scriptToDisableMusic;
    [SerializeField] private GameObject scriptToDisableSound;
    [SerializeField] private GameObject scriptToDisableMBM;
    [SerializeField] private GameObject scriptScore;
    private AudioSource soundController;
    private AudioSource musicController;
    private MenuButtonManager mbm;

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

        time = 3;
        secPassed = 0;
        UpdateTimer();

        if (scriptScore != null)
        {
            score = scriptScore.GetComponent<Score>();
        }

        if (scriptToDisableSound != null)
        {
            soundController = scriptToDisableSound.GetComponent<AudioSource>();
        }

        if (scriptToDisableMusic != null)
        {
            musicController = scriptToDisableMusic.GetComponent<AudioSource>();
        }
        if (scriptToDisableMBM != null)
        {
            mbm = scriptToDisableMBM.GetComponent<MenuButtonManager>();
        }
    }
    public void StopSound()
    {
        if (soundController != null)
        {
            soundController.enabled = false;
        }
    }

    public void StopMusic()
    {
        if (musicController != null)
        {
            musicController.enabled = false;
        }
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
            //OpenFinalCanvas();
            finScore = score.score;
            highScoreFin = score.highScore;
            cargoDeliveredFin = score.cargoDelivered;
            Debug.Log("Time over");
            mbm.LoadFinalScene();
            taskText.TimesUpMessage();
        }
        if (time > 0 && secPassed >= 0 && secPassed % 3 == 0)
        {
            onSecPassed?.Invoke();
            //Debug.Log("Sec passed, cargo spawn event fired");
        }
    }

    //private void OpenFinalCanvas()
    //{
    //    StopSound();
    //    StopMusic();
    //    canvas.gameObject.SetActive(true);
    //    mbm.gameObject.SetActive(false);

    //    Cursor.lockState = CursorLockMode.Locked;
    //    Cursor.visible = false;
    //}

    // Update is called once per frame
    void Update()
    {

    }
}
