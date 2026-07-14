using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CargoSpawner : MonoBehaviour
{
    public List<CargoSpawnZone> cargoZones = new List<CargoSpawnZone>();
    public List<CargoSpawnZone> activeZones = new List<CargoSpawnZone>();
    public GameObject smallPrefab;
    public GameObject bigPrefab;
    public GameObject longPrefab;
    public GameObject canisterPrefab;
    public GameObject barrelPrefab;
    public GameObject bottlePrefab;

    private CargoSpawnZone activeZone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        FindZones();
        ActivateAll();
        //DeactivateAll();
        //ActivateRandom();

        Timer timer = FindFirstObjectByType<Timer>();
        timer.onSecPassed += SpawnRandomInActive;
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
        activeZone = null;
    }
    public void DeactivateZone(GameObject zone)
    {
        if (cargoZones.Contains(zone.GetComponent<CargoSpawnZone>()))
        {
            zone.SetActive(false);
        }
        //activeZoneExists = false;
        activeZone = null;
    }
    public void ActivateRandom()
    {
        //if (!activeZoneExists)  //only one active at a time for now
        //{
        UpdateActiveZones();
        if (activeZones.Count == cargoZones.Count) return;

        int idx = Random.Range(0, cargoZones.Count);
        while (activeZones.Contains(cargoZones[idx]))
        {
            idx = (idx+1)%cargoZones.Count;
        }
        cargoZones[idx].gameObject.SetActive(true);
        activeZone = cargoZones[idx];

        UpdateActiveZones();

        //activeZoneExists = true;
        //}
        //else return;
    }
    public void SpawnRandomInActive()
    {
        Debug.Log("SpawnRandom in active called");
        GameObject[] cargos = new GameObject[] { smallPrefab, bigPrefab, longPrefab, bottlePrefab, barrelPrefab, canisterPrefab };
        foreach (CargoSpawnZone zone in activeZones)
        {
            int idx = Random.Range(0, cargos.Length);
            zone.SpawnCargo(cargos[idx]);
            
        }
    }
    void UpdateActiveZones()
    {
        foreach (CargoSpawnZone zone in cargoZones)
        {
            if (zone.gameObject.activeInHierarchy)
                activeZones.Add(zone);
        }
    }
    public void GetCargoZones(out Transform[] res)
    {
        Transform[] del = new Transform[cargoZones.Count];
        int i = 0;
        foreach (CargoSpawnZone zone in cargoZones)
        {
            del[i] = zone.transform; i++;
        }
        res = del;
    }
    public void GetCargoZones(out List<CargoSpawnZone> res)
    {
        res = cargoZones;
    }
    public void GetActiveCargoZones(out Transform[] res)
    {
        Transform[] del = new Transform[activeZones.Count];
        int i = 0;
        foreach (CargoSpawnZone zone in activeZones)
        {
            del[i] = zone.transform; i++;
        }
        res = del;
    }
    public void ActivateAll()
    {
        foreach (CargoSpawnZone zone in cargoZones)
        {
            zone.gameObject.SetActive(true);
        }
        //activeZoneExists = false;
        //activeZone = null;
        UpdateActiveZones();
    }
    public void GetActiveCargoZones(out List<CargoSpawnZone> res)
    {
        res = activeZones;
    }
    public CargoSpawnZone GetActiveZone()
    {
        return activeZone;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
