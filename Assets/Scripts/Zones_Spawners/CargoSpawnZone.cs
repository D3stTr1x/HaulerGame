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
        pos.position = new Vector3(
                transform.position.x + Random.Range(-transform.localScale.x/2, transform.localScale.x/2),
                transform.position.y + pos.localScale.y + 1,
                transform.position.z + Random.Range(-transform.localScale.z/2, transform.localScale.z/2)
            );

        GameObject newCargo = Instantiate(cargo);
        newCargo.transform.position = pos.position;
        newCargo.transform.localScale = pos.localScale;
    }

    void Update()
    {
        
    }
}
