using UnityEngine;

public class CargoCanister : CargoBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 25f;
        base.Start();
    }
}
