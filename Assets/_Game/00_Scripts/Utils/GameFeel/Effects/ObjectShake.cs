using UnityEngine;

namespace Slafurry.Utils.GameFeel
{
    public class ObjectShake : MonoBehaviour, IGameFeelEffect
    {
        [Header("Jitter Settings")]
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private float intensity = 0.2f;
        [SerializeField] private bool dampening = true;

        [Header("Axis Settings")]
        [SerializeField] private bool shakeX = true;
        [SerializeField] private bool shakeY = true;
        [SerializeField] private bool shakeZ = false;

        [Header("Objects To Shake")]
        [SerializeField] private Transform[] gameObjects;

        private float currentDuration;
        private float currentIntensity;
        private bool isShaking;

        private Vector3[] currentOffsets;

        private void Awake()
        {
            currentOffsets = new Vector3[gameObjects.Length];
        }

        public void PlayEffect()
        {
            if (gameObjects == null || gameObjects.Length == 0)
                return;

            if (currentOffsets == null || currentOffsets.Length != gameObjects.Length)
                currentOffsets = new Vector3[gameObjects.Length];

            currentDuration = duration;
            currentIntensity = intensity;
            isShaking = true;
        }

        public void PlayEffect(float customDuration, float customIntensity)
        {
            duration = customDuration;
            intensity = customIntensity;
            PlayEffect();
        }

        public void StopEffect()
        {
            if (!isShaking)
                return;

            isShaking = false;

            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] == null)
                    continue;

                gameObjects[i].transform.localPosition -= currentOffsets[i];
                currentOffsets[i] = Vector3.zero;
            }
        }

        private void LateUpdate()
        {
            if (!isShaking)
                return;

            if (currentDuration <= 0f)
            {
                StopEffect();
                return;
            }

            float currentMag = currentIntensity;

            if (dampening)
            {
                float normalizedTime = currentDuration / duration;
                currentMag = Mathf.Lerp(0f, currentIntensity, normalizedTime);
            }

            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] == null)
                    continue;

                Transform t = gameObjects[i].transform;

                Vector3 rand = Random.insideUnitSphere * currentMag;

                Vector3 newOffset = new Vector3(
                    shakeX ? rand.x : 0f,
                    shakeY ? rand.y : 0f,
                    shakeZ ? rand.z : 0f
                );

                t.localPosition = (t.localPosition - currentOffsets[i]) + newOffset;
                currentOffsets[i] = newOffset;
            }

            currentDuration -= Time.deltaTime;
        }
    }
}