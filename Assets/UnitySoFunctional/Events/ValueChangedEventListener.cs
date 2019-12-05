using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class ValueChangedEventListener<T> : SerializedMonoBehaviour
    {
        protected ValueChangedEvent<T> Event;

        public Action<T> ResponseAction;

        [SerializeField]
        private IValueChanged<T> _variable;

        private void Awake()
        {
            this.Event = _variable.ValueChangedEvent;
        }

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