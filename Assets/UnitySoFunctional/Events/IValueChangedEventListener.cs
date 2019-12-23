using System.Collections.ObjectModel;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public interface IValueChangedEventListener
    {
        ReadOnlyCollection<UnityEventBase> Responses { get; }
        void Awake();
        void OnEnable();
        void OnDisable();
    }
}