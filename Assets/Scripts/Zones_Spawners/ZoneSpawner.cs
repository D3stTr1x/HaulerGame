using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSpawner : MonoBehaviour
{
    //private GameObject[] deliveryZones;
    public List<DeliveryZone> deliveryZones = new List<DeliveryZone>();
    private bool activeZoneExists;

    public TruckCargoSystem truckCargoSystem;
    private NavigationSystem navigationSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Awake()
//=======
    void Start()
    {
        // Находим навигатор на сцене, иначе он был равен null
        navigationSystem = Object.FindFirstObjectByType<NavigationSystem>();

        FindZones();
        DeactivateAll();
        activeZoneExists = false;
        truckCargoSystem = GameObject.FindGameObjectWithTag("Player").GetComponent<TruckCargoSystem>();
    }

    public void DeactivateZone(GameObject zone)
    {
        if (deliveryZones.Contains(zone.GetComponent<DeliveryZone>()))
        {
            zone.SetActive(false);
        }
        activeZoneExists = false;

        // Заставляем навигатор пересчитать цель сразу после отключения зоны
        if (navigationSystem != null)
        {
            navigationSystem.FindNearestTarget();
        }
    }
    void FindZones()
    {
        GameObject[] zones = GameObject.FindGameObjectsWithTag("DeliveryZone");
        foreach (GameObject zone in zones)
        {
            deliveryZones.Add(zone.GetComponent<DeliveryZone>());
        }
        Debug.Log($"Delivery zones found: {deliveryZones.Count}");
    }
    public void DeactivateAll()
    {
        //foreach (GameObject zone in deliveryZones)
        //{
        //    zone.SetActive(false);
        //}
        foreach (DeliveryZone zone in deliveryZones)
        {
            zone.gameObject.SetActive(false);
        }
        navigationSystem.FindNearestTarget();
        activeZoneExists = false;
    }
    //public void WaitAndDeactivateAll()
    //{
    //    DelayDeactivateAll();
    //    return;
    //}
    //public IEnumerator DelayDeactivateAll()
    //{
    //    Debug.Log("3 sec wait");
    //    yield return new WaitForSeconds(0.5f);
    //    Debug.Log("Deactivating zones");
    //    DeactivateAll();
    //}
    public void ActivateRandom()
    {
        if (!activeZoneExists)  //only one active at a time for now
        {
            int idx = Random.Range(0, deliveryZones.Count);
            deliveryZones[idx].gameObject.SetActive(true);
            navigationSystem.FindNearestTarget();
            activeZoneExists = true;
        }
        else return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetDeliveryZones(out Transform[] res)
    { 
        Transform[] del = new Transform[deliveryZones.Count];
        int i = 0;
        foreach (DeliveryZone zone in deliveryZones)
        {
            del[i] = zone.transform; i++;
        }
        res = del;
    }
    public void GetDeliveryZones(out List<DeliveryZone> res)
    {
        res = deliveryZones;
    }
}
