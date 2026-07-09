
using UnityEngine;

public class CarFlipRecover : MonoBehaviour
{
    [Header("Настройки переворота")]
    [Tooltip("Чем ближе к 1, тем при меньшем наклоне сработает переворот (0.5 = 60 градусов)")]
    public float flipThreshold = 0.5f;
    public float flipForce = 8500f;
    public float cooldownTime = 3f;
    public float uprightTorque = 1800f;

    [Header("Защита от ложных срабатываний на склонах")]
    public float groundCheckDistance = 2.2f;
    [Tooltip("Насколько ровно машина должна стоять к поверхности, чтобы не перевернуться (0.8 = колеса почти лежат на склоне)")]
    public float slopeAlignmentTolerance = 0.8f;

    private Rigidbody rb;
    private float cooldownTimer = 0f;
    private bool isFlipping = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (rb == null || cooldownTimer > 0 || isFlipping) return;

        // Vector3.Dot возвращает 1, если машина стоит ровно, 0 если на боку, -1 если на крыше
        float uprightDot = Vector3.Dot(transform.up, Vector3.up);

        // Если наклон превышает допустимый (машина завалена)
        if (uprightDot < flipThreshold)
        {
            // Проверяем, не едем ли мы просто по очень крутой горе
            if (IsSafelyOnSlope())
            {
                return; // Отменяем переворот, мы просто на склоне
            }

            PerformFlip();
        }
    }

    private bool IsSafelyOnSlope()
    {
        // Пускаем луч строго вниз (в мировых координатах), чтобы найти поверхность
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            // Проверяем, насколько "крыша" машины совпадает с нормалью (углом) склона.
            float slopeAlignment = Vector3.Dot(transform.up, hit.normal);

            // Если крыша смотрит туда же, куда и нормаль горы (колеса на земле), значит мы стабильны
            if (slopeAlignment > slopeAlignmentTolerance)
            {
                return true;
            }
        }

        // Поверхности под нами нет, ИЛИ мы лежим к ней боком (slopeAlignment маленький) — нужно перевернуть!
        return false;
    }

    private void PerformFlip()
    {
        isFlipping = true;
        cooldownTimer = cooldownTime;

        // Подкидываем машину вверх
        rb.AddForce(Vector3.up * flipForce, ForceMode.Impulse);

        // Вращаем ее, чтобы поставить на колеса
        Vector3 currentUp = transform.up;
        Vector3 axis = Vector3.Cross(currentUp, Vector3.up);
        if (axis.magnitude > 0.01f)
        {
            rb.AddTorque(axis.normalized * uprightTorque, ForceMode.Impulse);
        }

        // Слегка гасим текущее хаотичное вращение
        rb.angularVelocity *= 0.3f;

        Invoke(nameof(EndFlip), 1.8f);
    }

    private void EndFlip()
    {
        isFlipping = false;
        if (rb != null) rb.angularVelocity *= 0.6f;
    }

    [ContextMenu("Force Flip")]
    public void ForceFlip() => PerformFlip();
}