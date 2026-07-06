using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Generic drag-and-drop for any UI element (inventory slot, reorderable
    /// list item, etc). Fires events instead of containing game-specific
    /// drop logic - the listener decides what a valid drop target means.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float dimAlphaWhileDragging = 0.6f;

        public event Action OnDragStarted;
        public event Action<PointerEventData> OnDragEnded;

        private RectTransform _rectTransform;
        private Vector2 _originalAnchoredPosition;
        private Transform _originalParent;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalAnchoredPosition = _rectTransform.anchoredPosition;
            _originalParent = transform.parent;

            // reparent to canvas root so it renders above everything else while dragging
            transform.SetParent(rootCanvas.transform, worldPositionStays: true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = dimAlphaWhileDragging;
                canvasGroup.blocksRaycasts = false;
            }

            OnDragStarted?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            OnDragEnded?.Invoke(eventData);
        }

        /// <summary>Call this from your drop-target listener if the drop was invalid, to snap back.</summary>
        public void ReturnToOriginalPosition()
        {
            transform.SetParent(_originalParent, worldPositionStays: false);
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
        }
    }
}