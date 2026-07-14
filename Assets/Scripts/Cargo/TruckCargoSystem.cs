using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class TruckCargoSystem : MonoBehaviour
{
    [Header("Настройки")]
    //[SerializeField] public Transform cargoHoldPoint;
    //[SerializeField] private int maxCargoCount = 1;
    //private CargoPickup cargoPickup;
    //public float massCargo = 0f;
    [SerializeField] private Transform cargoHoldPoint;
    [SerializeField] private int maxCargoCount = 15;

    [Header("Размеры кузова (для сканирования)")]
    [Tooltip("Ширина кузова")]
    [SerializeField] private float bedWidth = 2.4f;
    [Tooltip("Длина кузова")]
    [SerializeField] private float bedLength = 3.6f;
    [Tooltip("Максимальная высота укладки")]
    [SerializeField] private float maxHeight = 2.5f;
    [Tooltip("Шаг сканирования (меньше = точнее, но тяжелее для ПК)")]
    [SerializeField] private float scanStep = 0.1f; // Уменьшили шаг для большей точности поиска мест

    public float totalMassCargo = 0f;
    public List<Transform> loadedCargos = new List<Transform>();

    private Dictionary<Transform, Vector3> allocatedTargets = new Dictionary<Transform, Vector3>();

    public bool CanPickupCargo() => loadedCargos.Count < maxCargoCount;
    public Transform GetCargoHoldPoint() => cargoHoldPoint;
    private NavigationSystem nav;

    private void Start()
    {
        nav = Object.FindFirstObjectByType<NavigationSystem>();
    }

    public void LoadCargo(Transform cargo)
    {
        if (loadedCargos.Contains(cargo)) return;
        loadedCargos.Add(cargo);

        CargoPickup pickup = cargo.GetComponent<CargoPickup>();

        // ИСПРАВЛЕНИЕ: Убираем прибавление массы отсюда. 
        // Масса будет добавлена только после того, как груз окажется в кузове.
        // if (pickup != null) totalMassCargo += pickup.massCargo; 

//<<<<<<< HEAD
//        if (nav != null) nav.SetDeliveryMode(true);
//=======
        //NavigationSystem nav = Object.FindFirstObjectByType<NavigationSystem>();
        //if (nav != null) nav.SetDeliveryMode(true);
    }

    public void UnloadCargo(Transform cargo)
    {
        if (!loadedCargos.Contains(cargo)) return;
        cargo.SetParent(null);
        loadedCargos.Remove(cargo);

        if (allocatedTargets.ContainsKey(cargo)) allocatedTargets.Remove(cargo);

        CargoPickup pickup = cargo.GetComponent<CargoPickup>();
        if (pickup != null) totalMassCargo -= pickup.massCargo;
    }

    public void ClearAllCargo()
    {
        foreach (Transform cargo in loadedCargos)
        {
            if (cargo != null)
            {
                cargo.SetParent(null);
                if (allocatedTargets.ContainsKey(cargo)) allocatedTargets.Remove(cargo);
                totalMassCargo = 0;
            }
            else { totalMassCargo = 0; }
        }
        loadedCargos.Clear();
    }

    public Vector3 GetDynamicCargoPosition(Transform newCargo)
    {
        Vector3 extents = GetCargoExtents(newCargo);

        float halfWidth = bedWidth / 2f;
        float halfLength = bedLength / 2f;

        for (float y = extents.y; y < maxHeight; y += scanStep)
        {
            for (float z = halfLength - extents.z; z > -halfLength + extents.z; z -= scanStep)
            {
                for (float x = -halfWidth + extents.x; x < halfWidth - extents.x; x += scanStep)
                {
                    Vector3 testPos = new Vector3(x, y, z);
                    Bounds testBounds = new Bounds(testPos, extents * 2f);

                    if (!IsIntersectingWithOtherCargo(testBounds, newCargo))
                    {
                        allocatedTargets[newCargo] = testPos;
                        return testPos;
                    }
                }
            }
        }

        Debug.LogWarning("Кузов забит! Кидаем груз сверху.");
        return new Vector3(0, maxHeight, 0);
    }

    private bool IsIntersectingWithOtherCargo(Bounds testBounds, Transform ignoreCargo)
    {
        // ИСПРАВЛЕНИЕ: Теперь мы уменьшаем размер проверки всего на 1 миллиметр (-0.001f).
        // Это не даст коробкам застревать, но и не позволит запихивать их в слишком узкие щели.
        testBounds.Expand(-0.001f);

        foreach (Transform cargo in loadedCargos)
        {
            if (cargo == ignoreCargo || cargo == null) continue;

            Bounds existingBounds = GetCargoLocalBounds(cargo);
            if (testBounds.Intersects(existingBounds))
            {
                return true;
            }
        }
        return false;
    }

    private Bounds GetCargoLocalBounds(Transform cargo)
    {
        Vector3 extents = GetCargoExtents(cargo);

        Rigidbody rb = cargo.GetComponentInParent<Rigidbody>();
        if (rb != null && rb.isKinematic && allocatedTargets.ContainsKey(cargo))
        {
            return new Bounds(allocatedTargets[cargo], extents * 2f);
        }

        Collider col = cargo.GetComponentInChildren<Collider>();
        Vector3 worldCenter = col != null ? col.bounds.center : cargo.position;
        Vector3 localCenter = cargoHoldPoint.InverseTransformPoint(worldCenter);
        return new Bounds(localCenter, extents * 2f);
    }


    private Vector3 GetCargoExtents(Transform cargo)
    {
        // Сначала пробуем найти стандартную коробку (как было раньше)
        BoxCollider box = cargo.GetComponentInChildren<BoxCollider>();
        if (box != null && !box.isTrigger)
        {
            return new Vector3(
                Mathf.Abs(box.size.x * box.transform.lossyScale.x),
                Mathf.Abs(box.size.y * box.transform.lossyScale.y),
                Mathf.Abs(box.size.z * box.transform.lossyScale.z)
            ) / 2f;
        }

        // ИСПРАВЛЕНИЕ 2: Если это канистра (MeshCollider, CapsuleCollider и т.д.)
        // Ищем ЛЮБОЙ коллайдер внутри груза, который НЕ является триггером.
        Collider[] colliders = cargo.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (!col.isTrigger)
            {
                // Возвращаем реальные границы физического объекта
                return col.bounds.extents;
            }
        }

        // Страховочный вариант, если коллайдер почему-то вообще не найден
        return Vector3.one * 0.5f;
    }

    // ВИЗУАЛИЗАЦИЯ ДЛЯ НАСТРОЙКИ
    private void OnDrawGizmosSelected()
    {
        if (cargoHoldPoint == null) return;

        Gizmos.matrix = cargoHoldPoint.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Голубой полупрозрачный цвет

        Vector3 center = new Vector3(0, maxHeight / 2f, 0);
        Vector3 size = new Vector3(bedWidth, maxHeight, bedLength);
        Gizmos.DrawWireCube(center, size);
    }
}