using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSpawner : MonoBehaviour
{
    //private GameObject[] deliveryZones;
    public List<DeliveryZone> deliveryZones = new List<DeliveryZone>();
    private bool activeZoneExists;
    private DeliveryZone activeZone;

    public TruckCargoSystem truckCargoSystem;
    private NavArrow nav;


    void Start()
    {
        nav = FindFirstObjectByType<NavArrow>();

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

        if (nav != null)
        {
            if (nav.GetTarget() == zone.transform) { nav.SetDeliveryMode(false); }
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
        foreach (DeliveryZone zone in deliveryZones)
        {
            zone.gameObject.SetActive(false);
        }
        activeZoneExists = false;
        activeZone = null;
    }
    public void ActivateRandom()
    {
        if (!activeZoneExists)  //only one active at a time for now
        {
            int idx = Random.Range(0, deliveryZones.Count);
            deliveryZones[idx].gameObject.SetActive(true);

            activeZoneExists = true;

            if (nav != null) { nav.SetDeliveryMode(true); nav.SetAsTarget(deliveryZones[idx].gameObject); }
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
    public DeliveryZone GetActiveZone()
    {
        return activeZone;
    }

}
