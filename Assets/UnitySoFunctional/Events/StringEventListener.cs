using System;
using System.Collections.Generic;
using DragonDogStudios.UnitySoFunctional.ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.UIElements;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class StringEventListener : ValueChangedEventListener<string>
    {
        public List<UnityEventBase> Responses = new List<UnityEventBase>();

        [Button]
        public void TestRaise()
        {
        }

        private void FireEvents(string arg)
        {
            foreach (var response in Responses)
            {
                (response as UnityEvent)?.Invoke();
                (response as UnityEvent<string>)?.Invoke(arg);
            }
        }
    }

    [Serializable]
    public class StringEvent : UnityEvent<string>
    {
    }
}