using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoadedCargoHandler : MonoBehaviour
{
    private GameObject player;
    private TruckCargoSystem truckCargoSystem;
    private ZoneSpawner zoneSpawner;

    private int numCargos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        truckCargoSystem = player.GetComponent<TruckCargoSystem>();
        zoneSpawner = GameObject.FindFirstObjectByType<ZoneSpawner>();

        truckCargoSystem.onCargoListSizeChanged += UpdateNumCargos;
        numCargos = 0;
    }
    private void Update()
    {
        //if ()
    }
    public int GetNumCargos()
    {
        UpdateNumCargos();
        return numCargos;
    }
    private void UpdateNumCargos()
    {
        numCargos = 0;
        List<Transform> curCargos = GetLoadedCargos();
        foreach (Transform t in curCargos)
        {
            if (t.IsDestroyed())
            {
                Debug.Log("Cargo was destroyed");
                curCargos.Remove(t);
            }
            else numCargos++;
        }

        //truckCargoSystem.loadedCargos = curCargos;
    }
    public List<Transform> GetLoadedCargos()
    {
        return truckCargoSystem.loadedCargos;
    }
    public void ClearAllCargo()
    {
        truckCargoSystem.ClearAllCargo();
    }
    //public void CheckDestroyed(int size)
    //{
        
    //}
    //{
    //public void DeactivateZones()
    //    Debug.Log("Zone deactivation started (loaded cargo handler)");
    //    //GetLoadedCargos();
    //    if (zoneSpawner)
    //    {
    //        if (numCargos == 0)
    //        {
    //            zoneSpawner.DeactivateAll();
    //        }
    //    }
    //}
}
