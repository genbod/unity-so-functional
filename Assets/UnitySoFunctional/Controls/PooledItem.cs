using System;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Controls
{
    public abstract class PooledItem : MonoBehaviour, IDisposable
    {
        public abstract RectTransform Transform { get; }

        public abstract void Dispose();
    }
}