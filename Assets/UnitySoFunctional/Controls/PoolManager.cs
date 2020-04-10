using System;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Controls
{
    public abstract class PoolManager : MonoBehaviour
    {
        public event Action PooledItemsUpdated;
        public abstract float LogItemHeight { get; }
        public abstract float Count { get; }

        protected void InvokePooledItemsUpdated()
        {
            PooledItemsUpdated?.Invoke();
        }
    }
}