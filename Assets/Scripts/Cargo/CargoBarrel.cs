using UnityEngine;

public class CargoBarrel : CargoBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 35f;
        base.Start();
    }
}
