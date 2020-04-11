using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Controls
{
    public class RecycledListView : MonoBehaviour
    {
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private RectTransform _viewportTransform;
        [SerializeField] private RectTransform _contentTransform;
        
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

        private void ClearContentTransformContents()
        {
            for (int i = 0; i < _contentTransform.childCount; i++)
            {
                GameObject.Destroy(_contentTransform.GetChild(i).gameObject);
            }
        }

        public void OnPooledItemsUpdated()
        {
            CalculateContentHeight();
            _viewportHeight = _viewportTransform.rect.height;
            UpdateItemsInTheList();
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

                CreatePoolItemsBetweenIndices(_currentBottomIndex + 1, newBottomIndex);
                DeletePoolItemsBetweenIndices(newBottomIndex + 1, _currentBottomIndex);
                CreatePoolItemsBetweenIndices(newTopIndex, _currentTopIndex - 1);
                DeletePoolItemsBetweenIndices(_currentTopIndex, newTopIndex - 1);
                
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

        private void DeletePoolItemAtIndex(int i)
        {
            _poolManager.RemoveItem(_pooledItemsAtIndex[i]);
        }

        private void CreatePoolItemsBetweenIndices(int topIndex, int bottomIndex)
        {
            for (int i = topIndex; i <= bottomIndex; i++)
                CreatePoolItemAtIndex(i);
        }

        private void CreatePoolItemAtIndex(int index)
        {
            Vector2 anchoredPosition = new Vector2( 0f, -index * _logItemHeight);
            var pooledItem = _poolManager.GetItem(index, _contentTransform, anchoredPosition);
            _pooledItemsAtIndex[index] = pooledItem;
        }
    }
}