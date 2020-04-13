using System;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Controls
{
    public abstract class PoolManager : MonoBehaviour
    {
        public event Action PooledItemsUpdated;
        public abstract float LogItemHeight { get; }
        public abstract int Count { get; }
        public abstract bool SnapToBottom { get; set; }

        protected void InvokePooledItemsUpdated()
        {
            PooledItemsUpdated?.Invoke();
        }

        public abstract PooledItem GetItem(int index, RectTransform contentTransform, Vector2 anchoredPosition);

        public abstract void RemoveItem(PooledItem pooledItem);
    }
}