using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unit = System.ValueTuple;

public class ToggleSetter : MonoBehaviour 
{
	public bool AlwaysUpdate;

	public bool ShouldInvert;

	public BoolVariable Toggled;

	public Toggle Toggle;

	public VoidGameEvent ToggleGameEvent;

	// Use this for initialization
	void Start () {
		Toggle.onValueChanged.AddListener(ToggleClicked);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (AlwaysUpdate)
		{
			Toggle.onValueChanged.RemoveAllListeners();
			Toggle.isOn = Toggled.GetValue() ^ ShouldInvert;
			Toggle.onValueChanged.AddListener(ToggleClicked);
		}
	}

	private void ToggleClicked(bool toggleValue)
	{
		ToggleGameEvent.Raise(new Unit());
		Toggled.SetValue(toggleValue ^ ShouldInvert);
	}
}
