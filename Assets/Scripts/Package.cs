using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Package : MonoBehaviour
{
    //healthbar govna ne rabotaet

    //public TextHP textHp;
    //public GameObject barPrefab;

    //private LineRenderer line;
    //public float barWidth = 1f;
    public GameObject barQuadPrefab;

    private float health;
    private float maxHealth = 10f;
    private QuadHP hpBar;

    private int penalty;
    private bool isDelivered = false;
    //private GameObject barInstance;
    //private Slider healthSlider;
    //private Vector3 start;
    //private Vector3 end;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        penalty = -((int) maxHealth / 10);
        health = maxHealth;
        SetQuadBar();
        //Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        //textHp.objectToFollow = gameObject;
        //SetLine();
        //SpawnBar();
    }

    void SetQuadBar()
    {
        if (barQuadPrefab == null)
        {
            Vector3 spPos = transform.position + Vector3.up * 1f;
            GameObject barObj = Instantiate(barQuadPrefab, spPos, Quaternion.identity);
            hpBar = barObj.GetComponent<QuadHP>();

            if (hpBar != null)
            {
                hpBar.Set(maxHealth, transform);
            }
        }
    }

    //void SetLine()
    //{
    //    line = GetComponent<LineRenderer>();
    //    line.positionCount = 2;
    //    start = transform.position + offset - Vector3.right * barWidth / 2;
    //    end = transform.position + offset + Vector3.right * barWidth / 2;
    //}
    //void SpawnBar()
    //{
    //    Vector3 barpos = transform.position + Vector3.up * 1.5f;
    //    barPrefab.transform.position = barpos;
    //    barInstance = Instantiate(barPrefab, barpos, Quaternion.identity);

    //    healthSlider = barInstance.GetComponentInChildren<Slider>();
    //    healthSlider.value = 1f;
    //}

    void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            UpdateScore(penalty);
            Die();
        }
    }

    //void UpdateLine()
    //{
    //    float hpPercent = health / maxHealth;
    //    end = start + Vector3.right * barWidth * hpPercent;

    //    line.SetPosition(0, start);
    //    line.SetPosition(1, end);
    //}

    //void UpdateSlider()
    //{
    //    if (healthSlider != null)
    //    {
    //        healthSlider.value = health / maxHealth;
    //    }
    //}
    void UpdateHealth()
    {
        if (hpBar != null)
            hpBar.UpdateHealth(health);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (isDelivered) return;

        float dmg = collision.relativeVelocity.magnitude;
        Debug.Log($"dmg taken: {dmg}, cur hp: {health}");

        TakeDamage(dmg);
        UpdateHealth();
        //textHp.UpdateText(maxHealth, health);
        //UpdateLine();
        //UpdateSlider();
    }
    void UpdateScore(int score)
    {
        if (SimpleScore.Instance != null)
        {
            SimpleScore.Instance.UpdateScore(score);
        }
    }
    void Die()
    {
        if (hpBar != null)
            Destroy(hpBar.gameObject);
        Destroy(gameObject);
        

        //Destroy(textHp);
        //Destroy(barInstance);

        Debug.Log($"package {gameObject} destroyed");
    }
    public void MarkDelivered()
    {
        isDelivered = true;
        Die();
        Debug.Log("package delivered");
    }
    // Update is called once per frame 
    void Update()
    {
        //UpdateLine();
        //if (line != null && Camera.main != null)
        //{
           
        //    line.transform.LookAt(Camera.main.transform);
        //}
    }
}
