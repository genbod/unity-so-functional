using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class ValueChangedEvent<T>
    {
        private readonly List<ValueChangedEventListener<T>> eventListeners = new List<ValueChangedEventListener<T>>();
        
        public void Raise(T arg)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised(arg);
            }
        }

        public void RegisterListener(ValueChangedEventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }

        public void UnregisterListener(ValueChangedEventListener<T> listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }
}