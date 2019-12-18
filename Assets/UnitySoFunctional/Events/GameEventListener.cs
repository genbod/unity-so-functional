using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class GameEventListener<T> : SerializedMonoBehaviour
    {

        [Tooltip("Event to register with.")]
        public GameEvent<T> Event;

        public Action<T> ResponseAction;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised(T arg)
        {
            ResponseAction(arg);
        }
    }
}