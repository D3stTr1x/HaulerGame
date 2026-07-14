using TMPro;
using UnityEngine;

public class TasksText : MonoBehaviour
{
    private TextMeshProUGUI tasksText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tasksText = gameObject.GetComponent<TextMeshProUGUI>();
        tasksText.text = "Current tasks:\n";
        //AddText();
    }
    void AddText(string text)
    {
        //tasksText.text = "Current tasks:\n";
        tasksText.text += text;
    }
    void FormDeliveryTask()
    {
        int rnd = (int)Random.Range(1, 11);
        string mes = $"Deliver Any cargo ({0}/{rnd})";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
