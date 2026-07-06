using UnityEngine;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Fits a RectTransform to Screen.safeArea, avoiding notches / punch-hole
    /// cameras on mobile. Attach to the top-level panel that holds critical
    /// UI (HUD, buttons) - not decorative background elements.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UISafeArea : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // Screen.safeArea can change at runtime (orientation change, foldable devices)
            if (Screen.safeArea != _lastSafeArea || Screen.orientation != _lastOrientation)
                ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastOrientation = Screen.orientation;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    }
}