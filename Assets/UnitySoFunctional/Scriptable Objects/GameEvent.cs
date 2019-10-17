using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

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
		for (int i = eventListeners.Count -1; i >= 0; i--)
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
