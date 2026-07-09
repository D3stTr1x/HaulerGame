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

    [Header("Pitch")]
    public float accelPitchMin = 0.85f;
    public float accelPitchMax = 2.2f;
    public float reversePitch = 1.0f;

    [Header("Brake Settings")]
    public float sharpBrakeMinSpeed = 20f;   // от какой скорости играть скрип
    public float sharpBrakeTime = 1.6f;
    public float normalBrakePitch = 0.78f;

    private AudioSource engineSource;
    private AudioSource brakeSource;

    private enum State { Idle, Accelerating, Braking, Reversing }
    private State currentState = State.Idle;

    private float brakeTimer = 0f;
    private bool wasBrakingLastFrame = false;
    private bool isReversing = false;           // запоминаем состояние заднего хода

    private void Awake()
    {
        engineSource = GetComponent<AudioSource>();
        engineSource.loop = true;
        engineSource.playOnAwake = false;

        var brakeObj = new GameObject("Brake Audio");
        brakeObj.transform.SetParent(transform);
        brakeSource = brakeObj.AddComponent<AudioSource>();
        brakeSource.loop = false;
        brakeSource.playOnAwake = false;
        brakeSource.volume = brakeVolume;
    }

    public void UpdateCarAudio(float throttle, float brakeInput, bool isReverseGear, float speed)
    {
        float absSpeed = Mathf.Abs(speed);

        // Приоритеты звуков
        if (brakeInput > 0.2f && absSpeed > 2f)
        {
            // Торможение (в приоритете, если скорость больше 2 км/ч)
            if (!wasBrakingLastFrame)
                StartBraking(absSpeed);

            SetState(State.Braking);
        }
        else if (isReverseGear && throttle > 0.05f)
        {
            // Задний ход играет ТОЛЬКО когда включена R и нажат газ
            SetState(State.Reversing);
        }
        else if (throttle > 0.08f)
        {
            // Обычный разгон вперед
            SetState(State.Accelerating);
        }
        else
        {
            // Машина катится или стоит (холостой ход)
            SetState(State.Idle);
        }

        wasBrakingLastFrame = brakeInput > 0.2f;

        UpdateEngineSound(throttle, absSpeed);
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

        switch (newState)
        {
            case State.Idle:
                ChangeEngineSound(idleClip, idleVolume, 1f);
                break;

            case State.Accelerating:
                ChangeEngineSound(accelClip, accelVolume, 1f);
                break;

            case State.Reversing:
                ChangeEngineSound(reverseClip, reverseVolume, reversePitch);
                break;

            case State.Braking:
                ChangeEngineSound(idleClip, idleVolume * 0.6f, 0.95f);
                break;
        }
    }

    private void ChangeEngineSound(AudioClip newClip, float volume, float pitch)
    {
        if (engineSource.clip != newClip)
        {
            engineSource.clip = newClip;
            engineSource.volume = volume;
            engineSource.pitch = pitch;
            engineSource.loop = true;
            engineSource.Play();
        }
        else
        {
            engineSource.volume = Mathf.Lerp(engineSource.volume, volume, 8f * Time.deltaTime);
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, pitch, 8f * Time.deltaTime);
        }
    }

    private void UpdateEngineSound(float throttle, float absSpeed)
    {
        if (currentState == State.Accelerating)
        {
            float target = Mathf.Lerp(accelPitchMin, accelPitchMax, absSpeed / 38f);
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, target, Time.deltaTime * 7f);
        }
        else if (currentState == State.Idle || currentState == State.Braking)
        {
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, 1f, Time.deltaTime * 6f);
        }
        else if (currentState == State.Reversing)
        {
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, reversePitch, Time.deltaTime * 4f);
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