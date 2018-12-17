using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnabledSetter : MonoBehaviour
{
    public bool AlwaysUpdate;

    public bool InvertValue;

    public BoolVariable Enabled;

    public GameObject go;

    private void OnEnable()
    {
        go.SetActive(Enabled.GetValue() ^ InvertValue);
    }

    // Update is called once per frame
    void Update ()
    {
        if (AlwaysUpdate)
        {
            var newEnabled = Enabled.GetValue() ^ InvertValue;
            if (go.activeSelf != newEnabled)
            {
               go.SetActive(newEnabled); 
            }
        }
	}
}
