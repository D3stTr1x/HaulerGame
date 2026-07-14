using UnityEngine;

public class CargoBottle : CargoBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 15f;
        base.Start();
    }
}
