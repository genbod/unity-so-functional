using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unit = System.ValueTuple;

public class Pause : MonoBehaviour 
{
	public VoidGameEvent PauseGameEvent;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.P))
		{
			PauseGameEvent.Raise(new Unit());
		}
	}
}
