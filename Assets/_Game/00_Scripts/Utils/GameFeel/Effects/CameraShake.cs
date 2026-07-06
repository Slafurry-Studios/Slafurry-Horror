using UnityEngine;
using Cinemachine;

namespace Slafurry.Utils.GameFeel
{
    public class CameraShake : MonoBehaviour, IGameFeelEffect

    {

        [Header("Impulse Source")]
        private CinemachineImpulseSource impulseSource;

        [Header("Default Shake Settings")]
        [SerializeField] private float defaultAmplitude = 1f;
        [SerializeField] private float defaultFrequency = 1f;

        private void Start()
        {
            if (impulseSource == null)
            {
                impulseSource = FindAnyObjectByType<CinemachineImpulseSource>();
                if (impulseSource == null)
                {
                    Debug.LogError("[CameraShakeManager] No CinemachineImpulseSource found on the GameObject!");
                }
            }
        }

        public void Shake(float amplitude, float frequency)
        {
            if (impulseSource == null)
            {
                Debug.LogWarning("[CameraShakeManager] Impulse Source not assigned!");
                return;
            }

            impulseSource.m_ImpulseDefinition.m_AmplitudeGain = amplitude;
            impulseSource.m_ImpulseDefinition.m_FrequencyGain = frequency;

            impulseSource.GenerateImpulse();
        }

        public void PlayEffect()
        {
            Shake(defaultAmplitude, defaultFrequency);
        }

        public void StopEffect()
        {
            if (impulseSource == null) return;

            impulseSource.m_ImpulseDefinition.m_AmplitudeGain = 0f;
        }
    }
}