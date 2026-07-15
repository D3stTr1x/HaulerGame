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

    [Header("UI")]
    public GameObject uiCanvas;
    public Slider healthSlider;
    public float showOnDamageDuration = 2f;
    public float showTimer = 0f;
    private Camera mainCamera;
    private float uiHeightOffset = 1f;

    public Image healthBarFill;
    public Color greenColor = new Color(0.2f, 0.8f, 0.2f);
    public Color yellowColor = new Color(0.9f, 0.8f, 0.1f);
    public Color redColor = new Color(0.8f, 0.2f, 0.2f);

    public float health;
    protected bool isDamageable;
    //protected float maxHealth;

    protected int penalty;
    [HideInInspector] public float maxHealth;

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
                    //Debug.Log($"Cargo delivered, events fired");
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

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        // Автоматически берем то здоровье, которое уже есть у груза, как максимальное
        maxHealth = health;
        FreezeHP();

        // Настраиваем ползунок под индивидуальное здоровье груза
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }

        UpdateHealthUI();
        HideUI();
        Init();
    }


    protected void Init()
    {
        penalty = -((int)health / 4);
        //health = maxHealth;
        pts = (int)health / 2;
    }
    void TakeDamage(float dmg)
    {
        if (isDamageable)
        {
            health -= dmg;
            //Debug.Log($"dmg taken: {dmg}, cur hp: {health}");
            UpdateHealthUI();
            if (healthSlider != null) { healthSlider.value = health; }
            showTimer = showOnDamageDuration;

            if (health <= 0)
            {
                if (Score.Instance != null)
                {
                    Score.Instance.UpdateScore(penalty);
                }
                Die();
            }
        }
        else
        {
            UnFreezeHP();
            return;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CargoSpawnZone")) return;
        if (IsDelivered) return;

        float dmg = collision.relativeVelocity.magnitude;
        if (dmg > 5)
        {
            TakeDamage(dmg);
            //if (!isDamageable) UnFreezeHP();
        }    
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
        HandleUIState();
        FaceCamera();
    }

    private void HandleUIState()
    {
        bool isTabPressed = Input.GetKey(KeyCode.Tab);
        if (showTimer > 0) showTimer-=Time.deltaTime;
        if (isTabPressed || showTimer > 0) ShowUI();
        else HideUI();
    }

    private void ShowUI()
    {
        if (uiCanvas != null && !uiCanvas.activeSelf) { uiCanvas.SetActive(true); Debug.Log("Active!"); }
    }

    private void HideUI()
    {
        if (uiCanvas != null && uiCanvas.activeSelf) uiCanvas.SetActive(false);
    }

    private void FaceCamera()
    {
        if (uiCanvas != null && uiCanvas.activeSelf && mainCamera != null)
        {
            // 1. Всегда держим Canvas ровно над объектом (по глобальной оси Y)
            uiCanvas.transform.position = transform.position + Vector3.up * uiHeightOffset;

            // 2. Поворачиваем к камере
            uiCanvas.transform.rotation = Quaternion.LookRotation(uiCanvas.transform.position - mainCamera.transform.position);
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider == null) return;

        // Записываем текущее здоровье в слайдер
        healthSlider.value = health;

        // Если привязан компонент Fill Image, меняем его цвет
        if (healthBarFill != null && maxHealth > 0f)
        {
            float healthPercent = health / maxHealth;

            if (healthPercent > 0.75f)
            {
                healthBarFill.color = greenColor;
            }
            else if (healthPercent >= 0.25f)
            {
                healthBarFill.color = yellowColor;
            }
            else
            {
                healthBarFill.color = redColor;
            }
        }
    }

    public void FreezeHP()
    {
        isDamageable = false;
        //Debug.Log("HP been frozen");
    }
    public void UnFreezeHP()
    {
        isDamageable = true;
        //Debug.Log("Unfrozen hp been");
    }
}
