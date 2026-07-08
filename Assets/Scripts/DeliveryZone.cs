using Unity.VisualScripting;
using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    private int pts = 30; //change later
    //public Color zoneColor = Color.yellow;

    private void OnTriggerEnter(Collider other)
    {
        Package box = other.GetComponent<Package>();
        if (box)
        {
            MarkDelivered(box);
            Destroy(other.gameObject);
        }
        else return;
    }
    void MarkDelivered(Package box) 
    {
        if (SimpleScore.Instance != null)
        {
            SimpleScore.Instance.UpdateScore(pts);
        }
        if (Timer.Instance != null)
        {
            Timer.Instance.AddTime();
        }
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
