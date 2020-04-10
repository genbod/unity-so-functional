using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Controls
{
    public abstract class PooledItem : MonoBehaviour
    {
        public abstract RectTransform Transform { get; }
    }
}