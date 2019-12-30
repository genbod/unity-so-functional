using System.Collections.ObjectModel;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public interface IEventListener
    {
        ReadOnlyCollection<UnityEventBase> Responses { get; }
        void Awake();
        void OnEnable();
        void OnDisable();
    }
}