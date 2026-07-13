using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class CargoBase : MonoBehaviour
{
    //healthbar govna ne rabotaet
    //public UnityEvent onPackagePickedUp;
    public UnityEvent onCargoDelivered;
    public int pts;

    public float health;
    //protected float maxHealth;

    protected int penalty;

    //private bool isPickedUp = false;
    private bool isDelivered = false;
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
                    Debug.Log($"Cargo delivered, events fired");
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //adding event listeners
        WarningText taskText = GameObject.FindFirstObjectByType<WarningText>();
        if (taskText != null)
        {
            onCargoDelivered.AddListener(taskText.PickupMessage);
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
    protected void Init()
    {
        penalty = -((int)health / 4);
        //health = maxHealth;
        pts = (int)health / 2;
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
        if (dmg > 5)
            TakeDamage(dmg);
    }
    void Die()
    {
        Destroy(gameObject);
        //Debug.Log($"cargo {gameObject} destroyed");
    }
    public void MarkDelivered()
    {
        IsDelivered = true;
        Die();
        //Debug.Log("cargo delivered");
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
