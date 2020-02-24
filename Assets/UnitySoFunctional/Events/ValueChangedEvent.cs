using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class ValueChangedEvent<T>
    {
        private readonly List<EventListener<T>> eventListeners = new List<EventListener<T>>();
        private readonly List<UnityAction<T>> unityActions = new List<UnityAction<T>>();
        
        public void Raise(T arg)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised(arg);
            }

            foreach (var action in unityActions)
            {
                action.Invoke(arg);
            }
        }

        public void RegisterAction(UnityAction<T> action)
        {
            if (!unityActions.Contains(action))
            {
                unityActions.Add(action);
            }
        }

        public void RegisterListener(EventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }

        public void UnregisterAction(UnityAction<T> action)
        {
            if (unityActions.Contains(action))
            {
                unityActions.Remove(action);
            }
        }

        public void UnregisterListener(EventListener<T> listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }
}