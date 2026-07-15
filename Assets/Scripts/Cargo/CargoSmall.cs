using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class CargoSmall : CargoBase
{
    //float maxHealth = 30f;
    void Start()
    {
        health = 10f;
        base.Start();
        //Init();
    }
}
