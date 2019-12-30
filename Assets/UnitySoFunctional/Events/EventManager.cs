using System;
using System.Collections.Generic;
using DragonDogStudios.UnitySoFunctional.ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class EventManager : SerializedMonoBehaviour
    {
        [SerializeField]
        private List<IEventListener> _eventListeners = new List<IEventListener>();

        private void Awake()
        {
            foreach (var eventListener in _eventListeners)
            {
                eventListener.Awake();
            }
        }

        private void OnEnable()
        {
            foreach (var eventListener in _eventListeners)
            {
                eventListener.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (var eventListener in _eventListeners)
            {
                eventListener.OnDisable();
            }
        }
    }
}