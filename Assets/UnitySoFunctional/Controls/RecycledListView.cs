using System;
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

        public void Awake()
        {
            _viewportHeight = _viewportTransform.rect.height;
            _logItemHeight = _poolManager.LogItemHeight;
            _1OverLogItemHeight = 1f / _logItemHeight;
            _poolManager.PooledItemsUpdated += OnPooledItemsUpdated;
        }

        private void OnPooledItemsUpdated()
        {
            CalculateContentHeight();
            // TODO: UpdateItemsInTheList
        }

        private void CalculateContentHeight()
        {
            float newHeight = Mathf.Max(1f, _poolManager.Count * _logItemHeight);
            _contentTransform.sizeDelta = new Vector2(0f, newHeight);
        }
    }
}