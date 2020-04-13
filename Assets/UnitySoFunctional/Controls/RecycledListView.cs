using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonDogStudios.UnitySoFunctional.Controls
{
    public class RecycledListView : MonoBehaviour
    {
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private RectTransform _viewportTransform;
        [SerializeField] private RectTransform _contentTransform;
        [SerializeField] private ScrollRect _scrollRect;
        
        private float _viewportHeight;
        private float _logItemHeight;
        private float _1OverLogItemHeight;
        private int _currentTopIndex = -1;
        private int _currentBottomIndex = -1;
        private Dictionary<int, PooledItem> _pooledItemsAtIndex = new Dictionary<int, PooledItem>();

        public void Awake()
        {
            _viewportHeight = _viewportTransform.rect.height;
            _logItemHeight = _poolManager.LogItemHeight;
            _1OverLogItemHeight = 1f / _logItemHeight;
            _poolManager.PooledItemsUpdated += OnPooledItemsUpdated;
            ClearContentTransformContents();
        }

        public void LateUpdate()
        {
            if (_poolManager.SnapToBottom)
            {
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void OnPooledItemsUpdated()
        {
            CalculateContentHeight();
            _viewportHeight = _viewportTransform.rect.height;
            UpdateItemsInTheList();
        }

        public void ScrollUp()
        {
            _poolManager.SnapToBottom = false;
            var newPos = Mathf.Min(_scrollRect.verticalNormalizedPosition + GetDeltaScroll(), 1);
            _scrollRect.verticalNormalizedPosition = newPos;
        }
        
        public void ScrollDown()
        {
            _poolManager.SnapToBottom = false;
            var newPos = Mathf.Max(_scrollRect.verticalNormalizedPosition - GetDeltaScroll(), 0);
            _scrollRect.verticalNormalizedPosition = newPos;
        }

        private float GetDeltaScroll()
        {
            var diff = Mathf.Max((_currentBottomIndex - _currentTopIndex) / 2, 1);
            var delta = _poolManager.Count != 0 ? (float)diff / _poolManager.Count : 0;
            return delta;
        }

        private void ClearContentTransformContents()
        {
            for (int i = 0; i < _contentTransform.childCount; i++)
            {
                GameObject.Destroy(_contentTransform.GetChild(i).gameObject);
            }
        }

        private void CalculateContentHeight()
        {
            float newHeight = Mathf.Max(1f, _poolManager.Count * _logItemHeight);
            _contentTransform.sizeDelta = new Vector2(0f, newHeight);
        }

        private void UpdateItemsInTheList()
        {
            if (_poolManager.Count > 0)
            {
                float contentPosTop = _contentTransform.anchoredPosition.y;
                float contentPosBottom = contentPosTop + _viewportHeight;

                int newTopIndex = Mathf.Max((int) (contentPosTop * _1OverLogItemHeight), 0);
                int newBottomIndex = Mathf.Min((int) (contentPosBottom * _1OverLogItemHeight),
                    (int) _poolManager.Count - 1);
                //print(
                //    $"curTop:{_currentTopIndex}, curBottom:{_currentBottomIndex}, newTop:{newTopIndex}, newBottom:{newBottomIndex}");
                //if (newTopIndex > _currentBottomIndex || newBottomIndex < _currentTopIndex) HardResetItems();

                DeletePoolItemsBetweenIndices(newBottomIndex + 1, _currentBottomIndex);
                DeletePoolItemsBetweenIndices(_currentTopIndex, newTopIndex - 1);
                CreatePoolItemsBetweenIndices(newTopIndex, newBottomIndex);
                
                _currentTopIndex = newTopIndex;
                _currentBottomIndex = newBottomIndex;
            }
        }

        private void DeletePoolItemsBetweenIndices(int topIndex, int bottomIndex)
        {
            if (topIndex < 0) return;
            for(int i = topIndex; i <= bottomIndex; i++)
                DeletePoolItemAtIndex(i);
        }

        private void DeletePoolItemAtIndex(int index)
        {
            // Only delete if there is an actual item at that index
            var item = GetItem(index);
            if (item != null)
            {
                _pooledItemsAtIndex[index] = null;
                _poolManager.RemoveItem(item);
            }
        }

        private PooledItem GetItem(int i)
        {
            PooledItem item = null;
            if (_pooledItemsAtIndex.TryGetValue(i, out item))
            {
                return item;
            }

            return null;
        }

        private void CreatePoolItemsBetweenIndices(int topIndex, int bottomIndex)
        {
            for (int i = topIndex; i <= bottomIndex; i++)
                CreatePoolItemAtIndex(i);
        }

        private void CreatePoolItemAtIndex(int index)
        {
            var item = GetItem(index);
            if (item != null) // If there is already an item there, make sure to remove it first
            {
                _poolManager.RemoveItem(item);
            }
            Vector2 anchoredPosition = new Vector2( 0f, -index * _logItemHeight);
            var pooledItem = _poolManager.GetItem(index, _contentTransform, anchoredPosition);
            _pooledItemsAtIndex[index] = pooledItem;
        }
    }
}