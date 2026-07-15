using System;
using Unity.VisualScripting;
using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    public event Action onMarkDelivered;
    //private int pts = 30; //change later
    private LoadedCargoHandler loadedCargoHandler;
    private ZoneSpawner zoneSpawner;
    private int numCargos;
    private int numDelivered;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (loadedCargoHandler)
            {
                numCargos = loadedCargoHandler.GetNumCargos();
            }
            Debug.Log($"Player entered trigger, numCargos: {numCargos}");
        }
        CargoBase box = other.GetComponent<CargoBase>();
        if (box)
        {
            MarkDelivered(box);
            numDelivered++;
            Debug.Log($"Cargo entered trigger, numDelivered: {numDelivered}");
            //Destroy(other.gameObject);
        }
        Debug.Log($"numCargos: {numCargos}, numDelivered: {numDelivered}");
        if (numCargos != 0 && numDelivered == numCargos)
        {
            DeactivateZone();
            if (!gameObject.activeInHierarchy)
                loadedCargoHandler.ClearAllCargo();
        }
    }
    public void DeactivateZone()
    {
        if (zoneSpawner)
        {
            zoneSpawner.DeactivateZone(gameObject);
            numCargos = 0;
            numDelivered = 0;
        }
    }
    void MarkDelivered(CargoBase box) 
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
        onMarkDelivered?.Invoke();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numCargos = 0;
        numDelivered = 0;
        loadedCargoHandler = GameObject.FindFirstObjectByType<LoadedCargoHandler>();
        zoneSpawner = GameObject.FindFirstObjectByType<ZoneSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
