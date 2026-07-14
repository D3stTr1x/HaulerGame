using UnityEngine;

public class CargoSpawnZone : MonoBehaviour
{
    //private CargoSpawner cargoSpawner;

    void Start()
    {
        //cargoSpawner = GameObject.FindFirstObjectByType<CargoSpawner>();
    }
    public void SpawnCargo(GameObject cargo)
    {
        Transform pos = cargo.transform;
        pos.position = new Vector3(transform.position.x + Random.Range(-7.5f, 7.5f), transform.position.y + pos.localScale.y + 1, transform.position.z + Random.Range(-7.5f, 7.5f));

        GameObject newCargo = Instantiate(cargo);
        newCargo.transform.position = pos.position;
        newCargo.transform.localScale = pos.localScale;
    }

    void Update()
    {
        
    }
}
