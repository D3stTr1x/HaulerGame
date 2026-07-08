using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Cargo : MonoBehaviour
{
    //healthbar govna ne rabotaet
    //public UnityEvent onPackagePickedUp;
    public UnityEvent onCargoDelivered;
    public int pts;

    private float health;
    private float maxHealth = 100f;

    private int penalty;

    //private bool isPickedUp = false;
    private bool isDelivered = false;

    //public bool IsPickedUp
    //{
    //    get => isPickedUp; 
    //    set 
    //    {
    //        if (isPickedUp != value)
    //        {
    //            isPickedUp = value;
    //            if (isPickedUp)
    //            {
    //                onPackagePickedUp?.Invoke();
    //                Debug.Log($"Package picked up, event fired");
    //            }
    //        }
    //    }
    //}
    public bool IsDelivered
    {
        get => isDelivered;
        set
        {
            if (isDelivered != value)
            {
                isDelivered = value;
                if (isDelivered)
                {
                    onCargoDelivered?.Invoke();
                    Debug.Log($"Package delivered, event fired");
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //adding event listeners
        TaskText taskText = GameObject.FindFirstObjectByType<TaskText>();
        if (taskText != null )
        {
            onCargoDelivered.AddListener(taskText.PickupMessage);
        }
        ZoneSpawner zoneSpawner = GameObject.FindFirstObjectByType<ZoneSpawner>();
        if (zoneSpawner != null)
        {
            onCargoDelivered.AddListener(zoneSpawner.DeactivateAll);
        }
        MinimapMarkers minimapMarkers = GameObject.FindFirstObjectByType<MinimapMarkers>();
        if (minimapMarkers != null)
        {
            onCargoDelivered.AddListener(minimapMarkers.DestroyMarkers);
        }
        Score score = GameObject.FindFirstObjectByType<Score>();
        if (score != null)
        {
            onCargoDelivered.AddListener(score.UpdateCargosDelivered);
        }
    }
    void Start()
    {
        penalty = -((int) maxHealth / 4);
        health = maxHealth;
        pts = (int)maxHealth / 2;
    }
    void TakeDamage(float dmg)
    {
        health -= dmg;
        Debug.Log($"dmg taken: {dmg}, cur hp: {health}");
        if (health <= 0)
        {
            if (Score.Instance != null)
            {
                Score.Instance.UpdateScore(penalty);
            }
            Die();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (IsDelivered) return;

        float dmg = collision.relativeVelocity.magnitude;
        if (dmg > 3)
            TakeDamage(dmg);
    }
    void Die()
    {
        Destroy(gameObject);
        Debug.Log($"cargo {gameObject} destroyed");
    }
    public void MarkDelivered()
    {
        IsDelivered = true;
        Die();
        Debug.Log("cargo delivered");
    }
    private void OnDestroy()
    {
        onCargoDelivered.RemoveAllListeners();
    }
    // Update is called once per frame 
    void Update()
    {
    }
}
