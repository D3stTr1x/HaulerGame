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
        Init();
    }
    //void TakeDamage(float dmg)
    //{
    //    health -= dmg;
    //    Debug.Log($"dmg taken: {dmg}, cur hp: {health}");
    //    if (health <= 0)
    //    {
    //        if (Score.Instance != null)
    //        {
    //            Score.Instance.UpdateScore(penalty);
    //        }
    //        Die();
    //    }
    //}
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (IsDelivered) return;

    //    float dmg = collision.relativeVelocity.magnitude;
    //    if (dmg > 3)
    //        TakeDamage(dmg);
    //}
    //void Die()
    //{
    //    Destroy(gameObject);
    //    //Debug.Log($"cargo {gameObject} destroyed");
    //}
    //public void MarkDelivered()
    //{
    //    IsDelivered = true;
    //    Die();
    //    //Debug.Log("cargo delivered");
    //}
    //private void OnDestroy()
    //{
    //    onCargoDelivered.RemoveAllListeners();
    //}
    // Update is called once per frame 
    //void Update()
    //{
    //}
}
