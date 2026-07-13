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
        //Debug.Log("Something spawned");
        //Transform pos = gameObject.transform;
        //pos.localScale = cargo.transform.localScale;
        Transform cargoTr = cargo.transform;
        Transform pos = cargo.transform;
        pos.position = transform.position;
        pos.position = new Vector3(pos.position.x + Random.Range(-7.5f, 7.5f), pos.position.y + cargoTr.localScale.y + 2, pos.position.z + Random.Range(-7.5f, 7.5f));
        //pos.Sca

        GameObject.Instantiate(cargo, pos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
