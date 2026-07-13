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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        FindZones();
        DeactivateAll();
        activeZoneExists = false;
        truckCargoSystem = GameObject.FindGameObjectWithTag("Player").GetComponent<TruckCargoSystem>();
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
            activeZoneExists = true;
        }
        else return;
    }

    public void DeactivateZone(GameObject zone)
    {
        if (deliveryZones.Contains(zone.GetComponent<DeliveryZone>()))
        {
            zone.SetActive(false);
        }
        activeZoneExists = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
