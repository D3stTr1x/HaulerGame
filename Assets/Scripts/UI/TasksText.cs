using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TasksText : MonoBehaviour
{
    private TextMeshProUGUI tasksText;
    private Score score;
      //??
    private List<string> anyDelTasks = new List<string>();
    private List<string> canisterDelTasks = new List<string>();
    private List<string> barrelDelTasks = new List<string>();
    private List<string> bottleDelTasks = new List<string>();

    //private List<List<string>> taskList = new List<List<string>>(); //я не знаю зачем я это делаю
    private List<string> taskList = new List<string>();
    private string anyTask="", canTask="", barTask="", botTask="";
    private int totAny = 0, totCan = 0, totBar = 0, totBot = 0;
    private int anyDelivered = 0, canDelivered = 0, barDelivered = 0, botDelivered = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tasksText = gameObject.GetComponent<TextMeshProUGUI>();
        //taskList.Add(anyDelTasks);
        //taskList.Add(canisterDelTasks);
        //taskList.Add(barrelDelTasks);
        //taskList.Add(bottleDelTasks);

        score = FindFirstObjectByType<Score>();


        FormDeliveryTask();
        FormBarrelDeliveryTask();
        FormBottleDeliveryTask();
        FormCanisterDeliveryTask();

        //RefreshTaskList();

        UpdateTasksDisplay();
        //AddText();
    }
    void AddText(string text)
    {
        tasksText.text += text;
    }
    void FormDeliveryTask()
    {
        int rnd = (int)Random.Range(1, 11);
        totAny = rnd;
        UpdateDeliveryTask();
    }
    void FormBarrelDeliveryTask()
    {
        int rnd = (int)Random.Range(1, 6);
        totBar = rnd;
        UpdateBarDeliveryTask();
    }
    void FormCanisterDeliveryTask()
    {
        int rnd = (int)Random.Range(1, 6);
        totCan = rnd;
        UpdateCanDeliveryTask();
    }
    void FormBottleDeliveryTask()
    {
        int rnd = (int)Random.Range(1, 6);
        totBot = rnd;
        UpdateBotDeliveryTask();
    }
    void UpdateDeliveryTask()
    {
        string mes;
        //Debug.Log($"Any: {anyDelivered}");
        if (anyDelivered >= totAny)
        {
            mes = "Task Completed!";
            int sc = 10 * totAny;
            score.UpdateScore(sc);
        }
        else
        {
            mes = $"Deliver any cargo: ({anyDelivered}/{totAny})";
        }
        anyTask = mes;
    }
    void UpdateCanDeliveryTask()
    {
        string mes;
        if (canDelivered >= totCan)
        {
            mes = "Task Completed!";
            int sc = 10 * totCan;
            score.UpdateScore(sc);
        }
        else
        {
            mes = $"Deliver canisters: ({canDelivered}/{totCan})";
        }
        canTask = mes;
    }
    void UpdateBarDeliveryTask()
    {
        string mes;
        if (barDelivered >= totBar)
        {
            mes = "Task Completed!";
            int sc = 10 * totBar;
            score.UpdateScore(sc);
        }
        else
        {
            mes = $"Deliver barrels: ({barDelivered}/{totBar})";
        }
        barTask = mes;
    }
    void UpdateBotDeliveryTask()
    {
        string mes;
        if (botDelivered >= totBot)
        {
            mes = "Task Completed!";
            int sc = 10 * totBot;
            score.UpdateScore(sc);
        }
        else
        {
            mes = $"Deliver gas bottles: ({botDelivered}/{totBot})";
        }
        botTask = mes;
    }
    public void OnAnyCargoDelivered(GameObject cargo)
    {
        anyDelivered++;
        if (cargo.TryGetComponent<CargoCanister>(out CargoCanister can))
        {
            canDelivered++;
        }
        if (cargo.TryGetComponent<CargoBarrel>(out CargoBarrel bar))
        {
            barDelivered++;
        }
        if (cargo.TryGetComponent<CargoBottle>(out CargoBottle bot))
        {
            botDelivered++;
        }
        Debug.Log($"onAnyCargoDel fired:   a: {anyDelivered}, c: {canDelivered}, b: {barDelivered}, bo: {botDelivered}   (TasksText)");
        UpdateTasksDisplay();
    }
    void UpdateTasksDisplay()
    {
        tasksText.text = "Current tasks:\n";

        UpdateDeliveryTask();
        UpdateCanDeliveryTask();
        UpdateBarDeliveryTask();
        UpdateBotDeliveryTask();

        RefreshTaskList();

        foreach (string s in taskList)
        {
            tasksText.text += $"{s}\n";
        }
        
        
        //for (int i = 0; i < taskList.Count; i++)
        //{
        //    if
        //}
    }
    void RefreshTaskList()
    {
        taskList.Clear();
        taskList.Add(anyTask);
        taskList.Add(canTask);
        taskList.Add(barTask);
        taskList.Add(botTask);
    }
    void Update()
    {
        
    }
}
