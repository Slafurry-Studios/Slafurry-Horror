using System.Collections.Generic;
using UnityEngine;

namespace Slafurry.Utils.GameFeel
{
    public class Blink : MonoBehaviour, IGameFeelEffect
    {
        [Header("Blink Settings")]
        [Tooltip("The color to blink to when hit (e.g. solid white or red).")]
        [SerializeField] private Color blinkColor = Color.white;

        [Tooltip("How long the blink lasts in seconds.")]
        [SerializeField] private float blinkDuration = 0.1f;

        [Tooltip("If true, it will find and blink all SpriteRenderers on this object and its children.")]
        [SerializeField] private bool includeChildren = true;

        [Header("Object to blink")]
        [SerializeField] private GameObject[] objects;
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;
        private float currentBlinkTimer;
        private bool isBlinking = false;

        private void Awake()
        {
            List<SpriteRenderer> rendererList = new();

            foreach (GameObject obj in objects)
            {
                if (obj == null) continue;

                if (includeChildren)
                    rendererList.AddRange(obj.GetComponentsInChildren<SpriteRenderer>(true));
                else if (obj.TryGetComponent(out SpriteRenderer renderer))
                    rendererList.Add(renderer);
            }

            spriteRenderers = rendererList.ToArray();
            originalColors = new Color[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
                originalColors[i] = spriteRenderers[i].color;
        }

        public void PlayEffect()
        {
            currentBlinkTimer = blinkDuration;

            if (!isBlinking)
            {
                isBlinking = true;

                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                    {
                        spriteRenderers[i].color = blinkColor;
                    }
                }
            }
        }

        public void PlayEffect(Color customColor, float customDuration)
        {
            blinkColor = customColor;
            blinkDuration = customDuration;
            PlayEffect();
        }

        public void StopEffect()
        {
            if (isBlinking)
            {
                isBlinking = false;

                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                    {
                        spriteRenderers[i].color = originalColors[i];
                    }
                }
            }
        }

        private void Update()
        {
            if (isBlinking)
            {
                currentBlinkTimer -= Time.deltaTime;

                if (currentBlinkTimer <= 0f)
                {
                    StopEffect();
                }
            }
        }
    }
}