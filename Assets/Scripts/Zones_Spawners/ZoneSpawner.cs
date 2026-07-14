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
//<<<<<<< HEAD
//    private NavigationSystem navigationSystem;
//    private CargoSpawner cargoSpawner;
//=======
    //private NavigationSystem navigationSystem;
    private NavArrow nav;
//>>>>>>> de943bc (new NavArrow)


    void Start()
    {
//<<<<<<< HEAD
//        // Ќаходим навигатор на сцене, иначе он был равен null 
//        navigationSystem = Object.FindFirstObjectByType<NavigationSystem>();
//        cargoSpawner = FindFirstObjectByType<CargoSpawner>();
//=======
        // Ќаходим навигатор на сцене, иначе он был равен null
        //navigationSystem = Object.FindFirstObjectByType<NavigationSystem>();
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
//<<<<<<< HEAD
//        activeZone = null;
//        // «аставл€ем навигатор пересчитать цель сразу после отключени€ зоны
//        if (navigationSystem != null)
//        {
//            //navigationSystem.FindNearestTarget();
//            //navigationSystem.SetActivePoint();
//        }

//        //prob shouldnt be there
//        if (cargoSpawner != null)
//        {
//            // а надо ли это вообще, может оставить их пусть всегда будут активны.
//            cargoSpawner.DeactivateAll();
//            cargoSpawner.ActivateRandom();
//=======

        if (nav != null)
        {
            if (nav.GetTarget() == zone.transform) { nav.SetDeliveryMode(false); }
        }
        // «аставл€ем навигатор пересчитать цель сразу после отключени€ зоны
        //if (navigationSystem != null)
        //{
        //    //navigationSystem.FindNearestTarget();
        //}
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
        //navigationSystem.FindNearestTarget();
        activeZoneExists = false;
        activeZone = null;
    }
    public void ActivateRandom()
    {
        if (!activeZoneExists)  //only one active at a time for now
        {
            int idx = Random.Range(0, deliveryZones.Count);
            deliveryZones[idx].gameObject.SetActive(true);
            //navigationSystem.FindNearestTarget();
//<<<<<<< HEAD
//            activeZoneExists = true;
//            activeZone = deliveryZones[idx];
//=======
            //navigationSystem.SetDeliveryPointTarget();
            //navigationSystem.SetAsTarget(deliveryZones[idx].gameObject);
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
