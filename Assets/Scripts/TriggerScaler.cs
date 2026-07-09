using UnityEngine;

public class TriggerScaler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
        if (gameObject.transform != null)
        {
            Vector3 scale = gameObject.transform.localScale;
            trigger.transform.localScale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
