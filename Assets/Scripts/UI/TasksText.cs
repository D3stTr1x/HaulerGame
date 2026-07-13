using TMPro;
using UnityEngine;

public class TasksText : MonoBehaviour
{
    private TextMeshProUGUI tasksText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tasksText = gameObject.GetComponent<TextMeshProUGUI>();
        UpdateTasksTextDisplay();
    }
    void UpdateTasksTextDisplay()
    {
        tasksText.text = "Current tasks:\n õç";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
