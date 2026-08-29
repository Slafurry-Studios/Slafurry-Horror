using UnityEngine;
using UnityEngine.EventSystems;

namespace Slafurry.Utils.UI
{
    public class BlockScrollDrag : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData) { }
    }
}
