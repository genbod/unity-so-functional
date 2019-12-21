using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class ValueChangedEventListener<T> : SerializedMonoBehaviour
    {
        protected ValueChangedEvent<T> Event;

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        private IValueChanged<T> _variable;

        [SerializeField] private bool _fireOnEnable = false;

        public List<UnityEventBase> Responses = new List<UnityEventBase>();

        private void Awake()
        {
            this.Event = _variable.ValueChangedEvent;
        }

        private void OnEnable()
        {
            Event.RegisterListener(this);
            if (_fireOnEnable)
            {
                _variable.FireEvent();
            }
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised(T arg)
        {
            foreach (var response in Responses)
            {
                (response as UnityEvent)?.Invoke();
                (response as UnityEvent<T>)?.Invoke(arg);
            }
        }
    }
}