using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ValueChangedEvent<T>
{
    private readonly List<ValueChangedEventListener<T>> eventListeners = new List<ValueChangedEventListener<T>>();

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
