using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateValue : MonoBehaviour {
    public ScriptableValue Value;

    public Text Text;

    public bool PrettyPrint;

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Text.text = Value.GetValueToString(PrettyPrint);	
	}
}
