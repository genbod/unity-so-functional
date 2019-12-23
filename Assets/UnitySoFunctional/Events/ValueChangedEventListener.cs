using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    [Serializable]
    public class ValueChangedEventListener<T> : IValueChangedEventListener
    {
        protected ValueChangedEvent<T> Event;

        [SerializeField, InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        private IValueChanged<T> _variable;

        [SerializeField]
        private bool _fireOnEnable = false;

        [SerializeField]
        private List<UnityEventBase> _responses = new List<UnityEventBase>();

        public ReadOnlyCollection<UnityEventBase> Responses => _responses.AsReadOnly();

        public void Awake()
        {
            this.Event = _variable.ValueChangedEvent;
        }

        public void OnEnable()
        {
            Event.RegisterListener(this);
            if (_fireOnEnable)
            {
                _variable.FireEvent();
            }
        }

        public void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised(T arg)
        {
            foreach (var response in _responses)
            {
                (response as UnityEvent)?.Invoke();
                (response as UnityEvent<T>)?.Invoke(arg);
            }
        }
    }
}