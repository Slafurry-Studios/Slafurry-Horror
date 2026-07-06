using UnityEngine;
using System.Collections;

namespace Slafurry.Utils.GameFeel
{
    public class SquashOnHit : MonoBehaviour, IGameFeelEffect
    {
        [SerializeField] private float squashX = 1.3f;
        [SerializeField] private float squashY = 0.7f;
        [SerializeField] private float duration = 0.15f;

        [Header("Objects to squash")]
        [SerializeField] private GameObject[] objects;

        private Vector3[] originalScales;

        private void Awake()
        {
            if (objects == null || objects.Length == 0)
                objects = new[] { gameObject };

            originalScales = new Vector3[objects.Length];

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    originalScales[i] = objects[i].transform.localScale;
            }
        }

        public void PlayEffect()
        {
            StopAllCoroutines();

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    originalScales[i] = objects[i].transform.localScale;
            }

            StartCoroutine(SquashRoutine());
        }

        public void StopEffect()
        {
            StopAllCoroutines();

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    objects[i].transform.localScale = originalScales[i];
            }
        }

        private IEnumerator SquashRoutine()
        {
            float half = duration * 0.5f;

            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / half;

                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] == null) continue;

                    Vector3 target = new Vector3(
                        originalScales[i].x * squashX,
                        originalScales[i].y * squashY,
                        originalScales[i].z);

                    objects[i].transform.localScale =
                        Vector3.Lerp(originalScales[i], target, t);
                }

                yield return null;
            }

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / half;

                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] == null) continue;

                    Vector3 target = new Vector3(
                        originalScales[i].x * squashX,
                        originalScales[i].y * squashY,
                        originalScales[i].z);

                    objects[i].transform.localScale =
                        Vector3.Lerp(target, originalScales[i], t);
                }

                yield return null;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    objects[i].transform.localScale = originalScales[i];
            }
        }
    }
}