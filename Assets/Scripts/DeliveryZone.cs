using Unity.VisualScripting;
using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    private int pts = 30; //change later
    //public Color zoneColor = Color.yellow;

    private void OnTriggerEnter(Collider other)
    {
        Cargo box = other.GetComponent<Cargo>();
        if (box)
        {
            MarkDelivered(box);
            //Destroy(other.gameObject);
        }
        else return;
    }
    void MarkDelivered(Cargo box) 
    {
        if (Score.Instance != null)
        {
            Score.Instance.UpdateScore(box.pts);
        }
        if (Timer.Instance != null)
        {
            Timer.Instance.AddTime();
        }
        box.MarkDelivered();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
