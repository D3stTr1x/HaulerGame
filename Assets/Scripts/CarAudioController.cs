
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarAudioController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip idleClip;
    public AudioClip accelClip;
    public AudioClip sharpBrakeClip;
    public AudioClip normalBrakeClip;
    public AudioClip reverseClip;

    [Header("Volumes")]
    [Range(0, 1)] public float idleVolume = 0.75f;
    [Range(0, 1)] public float accelVolume = 1f;
    [Range(0, 1)] public float brakeVolume = 1f;
    [Range(0, 1)] public float reverseVolume = 0.95f;

    [Header("Pitch Settings")]
    public float accelPitchMin = 0.85f;
    public float accelPitchMax = 2.2f;
    public float reversePitch = 1.0f;

    [Header("Rev Limiter (Отсечка)")]
    [Tooltip("Порог скорости для срабатывания отсечки (0.98 = 98% от макс. скорости передачи)")]
    public float revLimiterThreshold = 0.98f;
    [Tooltip("Скорость 'прыжков' звука при отсечке")]
    public float revLimiterSpeed = 30f;
    [Tooltip("Глубина падения питча при отсечке")]
    public float revLimiterPitchDrop = 0.35f;

    [Header("Brake Settings")]
    public float sharpBrakeMinSpeed = 20f;
    public float sharpBrakeTime = 1.6f;
    public float normalBrakePitch = 0.78f;

    [Header("Loop Smoothing (Сглаживание петли)")]
    [Tooltip("Время в секундах для плавного перетекания звука при зацикливании (наложении конца на начало)")]
    public float crossfadeDuration = 0.4f;

    // Два источника для бесшовного зацикливания двигателя
    private AudioSource engineSource1;
    private AudioSource engineSource2;
    private bool useSource1 = true; // Флаг, указывающий какой источник сейчас главный

    private AudioSource brakeSource;

    private enum State { Idle, Accelerating, Braking, Reversing }
    private State currentState = State.Idle;

    private float brakeTimer = 0f;
    private bool wasBrakingLastFrame = false;
    private float currentMaxGearSpeed = 50f;

    private void Awake()
    {
        // Настраиваем первый источник (основной)
        engineSource1 = GetComponent<AudioSource>();
        engineSource1.loop = false; // ВАЖНО: Нативное зацикливание выключаем!
        engineSource1.playOnAwake = false;

        // Автоматически создаем второй источник для сглаживания швов
        GameObject secondEngineObj = new GameObject("Engine Audio 2 (Seamless Loop)");
        secondEngineObj.transform.SetParent(transform);
        engineSource2 = secondEngineObj.AddComponent<AudioSource>();
        engineSource2.loop = false;
        engineSource2.playOnAwake = false;

        // Копируем настройки 3D-звука с первого компонента на второй
        CopyAudioSourceSettings(engineSource1, engineSource2);

        // Создаем источник для тормозов
        var brakeObj = new GameObject("Brake Audio");
        brakeObj.transform.SetParent(transform);
        brakeSource = brakeObj.AddComponent<AudioSource>();
        brakeSource.loop = false;
        brakeSource.playOnAwake = false;
        brakeSource.volume = brakeVolume;
    }

    private void CopyAudioSourceSettings(AudioSource from, AudioSource to)
    {
        to.spatialBlend = from.spatialBlend;
        to.minDistance = from.minDistance;
        to.maxDistance = from.maxDistance;
        to.rolloffMode = from.rolloffMode;
        to.outputAudioMixerGroup = from.outputAudioMixerGroup;
    }

    // Удобные помощники для определения активного в данный момент источника
    private AudioSource GetActiveSource() => useSource1 ? engineSource1 : engineSource2;
    private AudioSource GetInactiveSource() => useSource1 ? engineSource2 : engineSource1;

    private float GetTargetVolume()
    {
        switch (currentState)
        {
            case State.Idle: return idleVolume;
            case State.Accelerating: return accelVolume;
            case State.Reversing: return reverseVolume;
            case State.Braking: return idleVolume * 0.6f;
            default: return idleVolume;
        }
    }

    public void UpdateCarAudio(float throttle, float brakeInput, bool isReverseGear, float speed, float maxGearSpeedKmh)
    {
        float absSpeed = Mathf.Abs(speed);
        currentMaxGearSpeed = maxGearSpeedKmh;

        if (brakeInput > 0.2f && absSpeed > 2f)
        {
            if (!wasBrakingLastFrame) StartBraking(absSpeed);
            SetState(State.Braking);
        }
        else if (isReverseGear && throttle > 0.05f)
        {
            SetState(State.Reversing);
        }
        else if (throttle > 0.08f)
        {
            SetState(State.Accelerating);
        }
        else
        {
            SetState(State.Idle);
        }

        wasBrakingLastFrame = brakeInput > 0.2f;

        UpdateEngineSound(throttle, absSpeed);
        HandleSeamlessLoop(); // Вызов логики сглаживания петель
    }

    private void StartBraking(float currentSpeed)
    {
        brakeTimer = 0f;
        if (currentSpeed >= sharpBrakeMinSpeed && sharpBrakeClip != null)
        {
            brakeSource.clip = sharpBrakeClip;
            brakeSource.pitch = 1f;
            brakeSource.Play();
        }
    }

    private void SetState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        AudioClip targetClip = null;
        float targetPitch = 1f;

        switch (newState)
        {
            case State.Idle: targetClip = idleClip; targetPitch = 1f; break;
            case State.Accelerating: targetClip = accelClip; targetPitch = accelPitchMin; break;
            case State.Reversing: targetClip = reverseClip; targetPitch = reversePitch; break;
            case State.Braking: targetClip = idleClip; targetPitch = 0.95f; break;
        }

        ChangeEngineSound(targetClip, targetPitch);
    }

    private void ChangeEngineSound(AudioClip newClip, float initialPitch)
    {
        AudioSource active = GetActiveSource();
        AudioSource inactive = GetInactiveSource();

        if (active.clip != newClip)
        {
            // Если сменилось состояние машины, жестко тушим оба источника и запускаем активный
            active.Stop();
            inactive.Stop();

            active.clip = newClip;
            active.volume = GetTargetVolume();
            active.pitch = initialPitch;
            active.Play();
        }
    }

    private void UpdateEngineSound(float throttle, float absSpeed)
    {
        float speedKmh = absSpeed * 3.6f;
        float speedRatio = currentMaxGearSpeed > 0f ? Mathf.Clamp01(speedKmh / currentMaxGearSpeed) : 0f;

        AudioSource active = GetActiveSource();
        AudioSource inactive = GetInactiveSource();

        float targetPitch = active.pitch;
        float targetVolume = GetTargetVolume();

        if (currentState == State.Accelerating)
        {
            // 1. ЛОГИКА ИСКУССТВЕННОЙ ОТСЕЧКИ
            if (speedRatio >= revLimiterThreshold && throttle > 0.1f)
            {
                float bounce = Mathf.PingPong(Time.time * revLimiterSpeed, revLimiterPitchDrop);
                targetPitch = accelPitchMax - bounce;
                targetVolume = accelVolume - (bounce * 0.5f);

                // При отсечке мгновенно рвем звук активного источника
                active.pitch = targetPitch;
                if (!inactive.isPlaying) active.volume = targetVolume;
                return;
            }
            // 2. ОБЫЧНЫЙ РАЗГОН НА ПЕРЕДАЧЕ
            else
            {
                float calculatedPitch = Mathf.Lerp(accelPitchMin, accelPitchMax, speedRatio);
                targetPitch = Mathf.Lerp(active.pitch, calculatedPitch, Time.deltaTime * 7f);
                targetVolume = accelVolume;
            }
        }
        else if (currentState == State.Idle || currentState == State.Braking)
        {
            float calculatedPitch = (currentState == State.Braking) ? 0.95f : 1f;
            targetPitch = Mathf.Lerp(active.pitch, calculatedPitch, Time.deltaTime * 6f);
            targetVolume = GetTargetVolume();
        }
        else if(currentState == State.Reversing)
        {
            targetPitch = Mathf.Lerp(active.pitch, reversePitch, Time.deltaTime * 4f);
            targetVolume = reverseVolume;
        }

        // Обновляем питч активного источника
        active.pitch = targetPitch;

        // Синхронизируем питч неактивного источника, чтобы во время наложения звуки не фазили
        if (inactive.isPlaying)
        {
            inactive.pitch = targetPitch;
        }

        // Если кроссфейд сейчас НЕ идет, штатно управляем громкостью активного источника
        if (!inactive.isPlaying)
        {
            active.volume = Mathf.Lerp(active.volume, targetVolume, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// Магия бесшовного зацикливания
    /// </summary>
    private void HandleSeamlessLoop()
    {
        AudioSource active = GetActiveSource();
        AudioSource inactive = GetInactiveSource();

        if (active.clip == null || !active.isPlaying) return;

        float clipLength = active.clip.length;
        float currentTime = active.time;

        // Если до конца аудиофайла осталось меньше времени, чем длительность кроссфейда, 
        // и второй источник еще бездействует — запускаем второй источник с самого начала (0 сек)
        if (currentTime >= (clipLength - crossfadeDuration) && !inactive.isPlaying)
        {
            inactive.clip = active.clip;
            inactive.time = 0f;
            inactive.pitch = active.pitch;
            inactive.volume = 0f; // стартует из тишины
            inactive.Play();

            // Переключаем роли местами. Теперь "inactive" становится "active"
            useSource1 = !useSource1;
        }

        // Если оба источника сейчас играют (активный нарастает, старый затухает)
        if (engineSource1.isPlaying && engineSource2.isPlaying)
        {
            float targetVol = GetTargetVolume();
            float fadeSpeed = targetVol / crossfadeDuration;

            if (useSource1)
            {
                // Source1 нарастает, Source2 затухает
                engineSource1.volume = Mathf.MoveTowards(engineSource1.volume, targetVol, fadeSpeed * Time.deltaTime);
                engineSource2.volume = Mathf.MoveTowards(engineSource2.volume, 0f, fadeSpeed * Time.deltaTime);
                if (engineSource2.volume <= 0f) engineSource2.Stop();
            }
            else
            {
                // Source2 нарастает, Source1 затухает
                engineSource2.volume = Mathf.MoveTowards(engineSource2.volume, targetVol, fadeSpeed * Time.deltaTime);
                engineSource1.volume = Mathf.MoveTowards(engineSource1.volume, 0f, fadeSpeed * Time.deltaTime);
                if (engineSource1.volume <= 0f) engineSource1.Stop();
            }
        }
    }

    private void Update()
    {
        if (currentState == State.Braking)
        {
            brakeTimer += Time.deltaTime;
            if (brakeTimer > sharpBrakeTime && !brakeSource.isPlaying && normalBrakeClip != null)
            {
                brakeSource.clip = normalBrakeClip;
                brakeSource.pitch = normalBrakePitch;
                brakeSource.Play();
            }
        }
    }
}