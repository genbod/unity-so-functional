using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonDogStudios.UnitySoFunctional.Controls
{
    public class ScrollRectController : MonoBehaviour, IScrollHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private PoolManager _poolManager;

        public void OnScroll(PointerEventData eventData)
        {
            SetSnapToBottom();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _poolManager.SnapToBottom = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetSnapToBottom();
        }

        public void OnScrollbarDragStart(BaseEventData data)
        {
            _poolManager.SnapToBottom = false;
        }

        public void OnScrollbarDragEnd(BaseEventData data)
        {
            SetSnapToBottom();
        }

        private void SetSnapToBottom()
        {
            float scrollbarYPos = _scrollRect.verticalNormalizedPosition;
            _poolManager.SnapToBottom = scrollbarYPos <= 1E-6f;
        }
    }
}