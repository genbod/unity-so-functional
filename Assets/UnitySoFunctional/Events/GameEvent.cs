using Sirenix.OdinInspector;
using System.Collections.Generic;
using sfw.net;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    [CreateAssetMenu]
    public class GameEvent : SerializedScriptableObject
    {
        private readonly List<EventListener> _eventListeners = new List<EventListener>();

        [Button]
        public void Raise()
        {
            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(EventListener listener)
        {
            if (!_eventListeners.Contains(listener))
            {
                _eventListeners.Add(listener);
            }
        }

        public void UnregisterListener(EventListener listener)
        {
            if (_eventListeners.Contains(listener))
            {
                _eventListeners.Remove(listener);
            }
        }
    }
    
    public class GameEvent<T> : SerializedScriptableObject
    {
        private readonly List<GameEventListener<T>> eventListeners = new List<GameEventListener<T>>();

        [ShowInInspector]
        private T TestArg;

        [Button]
        private void TestRaise()
        {
            Raise(TestArg);
        }

        public void Raise(T arg)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised(arg);
            }
        }

        public void RegisterListener(GameEventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }

        public void UnregisterListener(GameEventListener<T> listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }
}