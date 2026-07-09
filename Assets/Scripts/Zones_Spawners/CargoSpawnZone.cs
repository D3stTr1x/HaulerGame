using UnityEngine;

public class CargoSpawnZone : MonoBehaviour
{
    private CargoSpawner cargoSpawner;
    //private GameObject smallPrefab;
    //private GameObject bigPrefab;
    //private GameObject longPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cargoSpawner = GameObject.FindFirstObjectByType<CargoSpawner>();
    }
    //public void SpawnSmallCargo()
    //{

    //}
    //public void SpawnBigCargo()
    //{

    //}
    //public void SpawnLongCargo()
    //{

    //}
    public void SpawnCargo(GameObject cargo)
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
