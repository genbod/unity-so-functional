using Sirenix.OdinInspector;
using System;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class ValueChangedEventListener<T> : SerializedMonoBehaviour
    {
        protected ValueChangedEvent<T> Event;

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