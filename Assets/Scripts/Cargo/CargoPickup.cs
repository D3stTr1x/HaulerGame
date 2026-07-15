
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CargoPickup : MonoBehaviour
{
    [Header("Íàñòðîéêè")]
    [SerializeField] private float pickupDistance = 6f;
    [SerializeField] private string pickupKey = "e";
    [SerializeField] private float fallDetectionHeight = 0.1f;
    [SerializeField] private float fallCheckInterval = 0.5f;
    [SerializeField] private float pickupDuration = 1.2f; // Âðåìÿ ïëàâíîãî ïîäúåìà
    [SerializeField] private TMP_Text HelpText;
    //[SerializeField] private AudioClip boxSound;
    /// <summary>
    /// [SerializeField] private AudioClip barrelSound;
    /// </summary>

    [Header("Âèçóàë")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.4f;


    public bool isTake = false;
    public float massCargo = 10f; // Óñòàíîâèòå áàçîâûé âåñ â èíñïåêòîðå

    private TruckCargoSystem truckSystem;
    private Renderer[] renderers;
    private Rigidbody rb;

    private static int lastPickupFrame = -1;
    private bool isPlayerNearby = false;
    private bool isHold = false;
    private float fallCheckTimer = 0f;

    private Transform currentCargoInTruck = null;
    public UnityEvent onCargoPickedUp;
    public bool isPickedUp;

    public bool IsPickedUp
    {
        get => isPickedUp;
        set
        {
            if (isPickedUp != value)
            {
                isPickedUp = value;
                if (isPickedUp)
                {
                    onCargoPickedUp?.Invoke();
                    Debug.Log($"Package picked up, event fired");
                }
            }
        }
    }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        rb = GetComponentInParent<Rigidbody>();

        //adding event listeners
        WarningText taskText = GameObject.FindFirstObjectByType<WarningText>();
        if (taskText != null)
        {
            onCargoPickedUp.AddListener(taskText.DeliverMessage);
        }
        ZoneSpawner zoneSpawner = GameObject.FindFirstObjectByType<ZoneSpawner>();
        if (zoneSpawner != null)
        {
            onCargoPickedUp.AddListener(zoneSpawner.ActivateRandom);
        }
        //NavigationSystem nav = FindFirstObjectByType<NavigationSystem>();
        //if (nav != null)
        //{
        //    onCargoPickedUp.AddListener(nav.SetDeliveryPointTarget);
        //}
        //NavArrow nav = FindFirstObjectByType<NavArrow>();
        //if (nav != null)
        //{
        //    onCargoPickedUp.AddListener()
        //}
    }
    private void Start()
    {
        SphereCollider sphereCollider = GetComponentInChildren<SphereCollider>();
        if (sphereCollider)
        {
            //float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            float maxScale = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            sphereCollider.radius = pickupDistance / maxScale;
        }
    }

    // ÈÑÏÐÀÂËÅÍÈÅ 1: Òåïåðü ìû ñ÷èòàåì êàæäóþ äåòàëü ãðóçîâèêà â çîíå, ÷òîáû ïðîñàäêà ïîäâåñêè íå ëîìàëà òðèããåð
    private HashSet<Collider> playersInZone = new HashSet<Collider>();

    // ... (Ñâîéñòâà è ìåòîäû Awake / Start îñòàþòñÿ áåç èçìåíåíèé)

    private void Update()
    {
        if (isPlayerNearby && !isHold)
        {
            if (Input.GetKeyDown(pickupKey))
            {
                TryPickup();
            }

            //PulseEffect();

            if (HelpText != null && !HelpText.gameObject.activeSelf)
            {
                ShowPickupPrompt(true);
            }
        }

        if (isHold)
        {
            fallCheckTimer += Time.deltaTime;
            if (fallCheckTimer >= fallCheckInterval)
            {
                CheckIfFallen();
                fallCheckTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Add(other); // Çàïîìèíàåì êîëëàéäåð, êîòîðûé âîøåë
            isPlayerNearby = true;

            if (truckSystem == null)
                truckSystem = other.GetComponentInParent<TruckCargoSystem>();

            if (!isHold) ShowPickupPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInZone.Remove(other); // Óäàëÿåì òîëüêî òîò êîëëàéäåð, êîòîðûé âûøåë

            // Çàùèòà: óäàëÿåì ïóñòûøêè, åñëè êàêàÿ-òî äåòàëü ìàøèíû áûëà óíè÷òîæåíà/îòêëþ÷åíà
            playersInZone.RemoveWhere(col => col == null || !col.gameObject.activeInHierarchy);

            // Èãðîê "óøåë" òîëüêî åñëè ÂÑÅ êîëëàéäåðû ïîêèíóëè çîíó
            if (playersInZone.Count == 0)
            {
                isPlayerNearby = false;
                if (!isHold) ShowPickupPrompt(false);
                ResetHighlight();
            }
        }
    }

    private void TryPickup()
    {
        // ÈÑÏÐÀÂËÅÍÈÅ 2: Çàùèòà îò êðàæè ââîäà ïåðåíåñåíà ñþäà. 
        // Åñëè êòî-òî â ýòîì êàäðå ÓÆÅ íà÷àë ïîäáîð, îñòàëüíûå îòìåíÿþòñÿ.
        if (lastPickupFrame == Time.frameCount) return;

        if (truckSystem == null) return;

        if (!truckSystem.CanPickupCargo())
        {
            Debug.Log("Êóçîâ óæå çàïîëíåí!");
            return;
        }

        if (isHold) return;

        // Áëîêèðóåì îñòàëüíûå ãðóçû ÒÎËÜÊÎ êîãäà òî÷íî óâåðåíû, ÷òî áåðåì ýòîò
        lastPickupFrame = Time.frameCount;

        ShowPickupPrompt(false);
        ResetHighlight();


        transform.GetComponent<CargoBase>().FreezeHP();
        truckSystem.LoadCargo(transform);
        StartCoroutine(WaitTimeTake(3f));
        StartCoroutine(MoveToCargoHold());
        IsPickedUp = true;

    }

    private System.Collections.IEnumerator WaitTimeTake(float delay)
    {
        isTake = true;
        yield return new WaitForSeconds(delay);
        isTake = false;
    }

    private System.Collections.IEnumerator HideHelpTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ÈÑÏÐÀÂËÅÍÈÅ: Ïîñëå òîãî êàê íàäïèñü "Ãðóç âûïàë" èñ÷åçíåò,
        // ïðîâåðÿåì, ñòîèì ëè ìû âñ¸ åù¸ ðÿäîì. Åñëè äà — âîçâðàùàåì ïîäñêàçêó î ïîäáîðå.
        if (isPlayerNearby && !isHold)
        {
            ShowPickupPrompt(true);
        }
        else if (HelpText != null)
        {
            HelpText.gameObject.SetActive(false);
        }
    }


    private void CheckIfFallen()
    {
        Transform holdPoint = truckSystem.GetCargoHoldPoint();
        if (holdPoint == null || !isHold) return;

        Vector3 cargoPos = transform.position;
        Vector3 holdPos = holdPoint.position;

        float maxDistanceX = 3.5f;

        // ÈÑÏÐÀÂËÅÍÈÅ 3: Áûëî 1.3f. Íî ïîëîâèíà äëèíû êóçîâà = 1.8f.
        // Ñòàâèì 3.5f, ÷òîáû ãðóçû íà êðàÿõ êóçîâà íå ñ÷èòàëèñü âûïàâøèìè!
        float maxDistanceZ = 3.5f;

        float maxDropY = 0.5f;

        float distX = Mathf.Abs(cargoPos.x - holdPos.x);
        float distZ = Mathf.Abs(cargoPos.z - holdPos.z);

        if (distX > maxDistanceX || distZ > maxDistanceZ || cargoPos.y < holdPos.y - maxDropY)
        {
            OnCargoFallen();
        }
    }


    private System.Collections.IEnumerator MoveToCargoHold()
    {
        Transform holdPoint = truckSystem.GetCargoHoldPoint();
        if (holdPoint == null) yield break;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            // ÈÑÏÐÀÂËÅÍÈÅ 1: Îòêëþ÷àåì ôèçè÷åñêèå ñòîëêíîâåíèÿ íà âðåìÿ ïîëåòà!
            // Òåïåðü ÿùèê - "ïðèçðàê" è íå ñìîæåò ïðîãíóòü ìàøèíó ñâîèì êîëëàéäåðîì.
            rb.detectCollisions = false;
        }

        transform.SetParent(null);

        // Çàïðàøèâàåì èäåàëüíîå ñâîáîäíîå ìåñòî
        Vector3 localGridOffset = truckSystem.GetDynamicCargoPosition(transform);
        Vector3 targetPos = holdPoint.TransformPoint(localGridOffset);
        Quaternion targetRot = holdPoint.rotation;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;

        // Ïëàâíîå ïåðåìåùåíèå
        while (elapsed < pickupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pickupDuration;

            // ÈÑÏÐÀÂËÅÍÈÅ 2: Äâèãàåì ÿùèê ïî êðàñèâîé äóãå
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            // Äîáàâëÿåì âûñîòó (ïðûæîê), ÷òîáû ãðóç ïåðåëåòàë ÷åðåç áîðò
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;

            transform.position = currentPos;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null; // Æäåì ñëåäóþùèé êàäð
        }

        // Æåñòêî ôèêñèðóåì â êîíöå ïóòè
        transform.position = targetPos;
        transform.rotation = targetRot;
        transform.SetParent(holdPoint);

        // Âêëþ÷àåì ôèçèêó îáðàòíî
        if (rb != null)
        {
            rb.isKinematic = false;
            // ÈÑÏÐÀÂËÅÍÈÅ 3: Âîçâðàùàåì ñòîëêíîâåíèÿ, êîãäà ãðóç óæå â êóçîâå
            rb.detectCollisions = true;
        }

        // Ïðèìåíÿåì ìàññó ãðóçà ê ãðóçîâèêó
        truckSystem.totalMassCargo += massCargo;

        isHold = true;
        currentCargoInTruck = transform;
        if (HelpText != null) HelpText.gameObject.SetActive(false);

    }

    private void OnCargoFallen()
    {
        isHold = false;
        currentCargoInTruck = null;

        if (rb != null)
        {
            rb.detectCollisions = true;
        }

        transform.SetParent(null);
        truckSystem.UnloadCargo(transform); // Âûãðóæàåì êîíêðåòíûé ãðóç èç ñèñòåìû

        if (HelpText != null)
        {
            HelpText.gameObject.SetActive(true);
            HelpText.text = "Ãðóç âûïàë èç êóçîâà!";
            StartCoroutine(HideHelpTextAfterDelay(3f));
        }
    }

    private void ResetHighlight()
    {
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_EmissionColor"))
                r.material.SetColor("_EmissionColor", Color.black);
        }
    }

    private void ShowPickupPrompt(bool show)
    {
        if (HelpText == null) return;

        if (show)
        {
            HelpText.text = $"Íàæìèòå [{pickupKey.ToUpper()}] ÷òîáû ïîãðóçèòü";
            HelpText.gameObject.SetActive(true);
        }
        else
        {
            HelpText.gameObject.SetActive(false);
        }
    }
}