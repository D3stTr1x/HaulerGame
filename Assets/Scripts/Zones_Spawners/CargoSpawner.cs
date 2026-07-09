using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CargoSpawner : MonoBehaviour
{
    private List<CargoSpawnZone> cargoZones = new List<CargoSpawnZone>();
    private List<CargoSpawnZone> activeZones = new List<CargoSpawnZone>();
    public GameObject smallPrefab;
    public GameObject bigPrefab;
    public GameObject longPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindZones();
        DeactivateAll();
    }
    void FindZones()
    {
        GameObject[] zones = GameObject.FindGameObjectsWithTag("CargoSpawnZone");
        foreach (GameObject zone in zones)
        {
            cargoZones.Add(zone.GetComponent<CargoSpawnZone>());
        }
    }
    public void DeactivateAll()
    {
        foreach (CargoSpawnZone zone in cargoZones)
        {
            zone.gameObject.SetActive(false);
        }
        //activeZoneExists = false;
    }
    public void DeactivateZone(GameObject zone)
    {
        if (cargoZones.Contains(zone.GetComponent<CargoSpawnZone>()))
        {
            zone.SetActive(false);
        }
        //activeZoneExists = false;
    }
    public void ActivateRandom()
    {
        //if (!activeZoneExists)  //only one active at a time for now
        //{
        GetActiveZones();
        if (activeZones.Count == cargoZones.Count) return;

        int idx = Random.Range(0, cargoZones.Count);
        while (activeZones.Contains(cargoZones[idx]))
        {
            idx = (idx+1)%cargoZones.Count;
        }
        cargoZones[idx].gameObject.SetActive(true);


        //activeZoneExists = true;
        //}
        //else return;
    }
    void GetActiveZones()
    {
        foreach (CargoSpawnZone zone in cargoZones)
        {
            if (zone.gameObject.activeInHierarchy)
                activeZones.Add(zone);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
