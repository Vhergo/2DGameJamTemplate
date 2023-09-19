using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [SerializeField] private float defaultShakeIntensity = 1f;
    [SerializeField] private float defaultShakeFrequency = 2f;
    [SerializeField] private float defaultShakeDuration = 0.25f;

    private CinemachineVirtualCamera vCam;
    private CinemachineBasicMultiChannelPerlin noise;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        vCam = GetComponent<CinemachineVirtualCamera>();
        noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    /// <summary>
    /// This method triggers a camera shake with the specified parameters.
    /// </summary>
    /// <param name="shakeIntensity">The intensity of the shake. If null, uses the default intensity.</param>
    /// <param name="shakeFrequency">The frequency of the shake. If null, uses the default frequency.</param>
    /// <param name="shakeDuration">The duration of the shake. If null, uses the default duration.</param>
    public void TriggerCameraShake(float shakeIntensity, float shakeFrequency, float shakeDuration) {
        ShakeCamera(shakeIntensity, shakeFrequency);
        Invoke("StopShake", shakeDuration);
    }

    /// <summary>
    /// Method overload for TriggerCameraShake()
    /// Call TriggerCameraShake() without parameters to trigger it with default values
    /// </summary>
    public void TriggerCameraShake() {
        TriggerCameraShake(defaultShakeIntensity, defaultShakeFrequency, defaultShakeDuration);
    }

    private void ShakeCamera(float shakeIntensity, float shakeFrequency) {
        noise.m_AmplitudeGain = shakeIntensity;
        noise.m_FrequencyGain = shakeFrequency;
    }

    private void StopShake() {
        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 1f;
    }
}
