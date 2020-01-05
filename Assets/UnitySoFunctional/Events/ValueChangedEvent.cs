using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class ValueChangedEvent<T>
    {
        private readonly List<EventListener<T>> eventListeners = new List<EventListener<T>>();
        
        public void Raise(T arg)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised(arg);
            }
        }

        public void RegisterListener(EventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
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