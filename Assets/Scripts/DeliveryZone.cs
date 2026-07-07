using Unity.VisualScripting;
using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    private int pts = 30; //change later
    public Color zoneColor = Color.yellow;

    private void OnTriggerEnter(Collider other)
    {
        Package box = other.GetComponent<Package>();
        MarkDelivered(box);
    }
    void MarkDelivered(Package box) 
    {
        if (SimpleScore.Instance != null)
        {
            SimpleScore.Instance.UpdateScore(pts);
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
