using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnabledSetter : MonoBehaviour
{
    public bool AlwaysUpdate;

    public BoolVariable Enabled;

    public GameObject go;

    private void OnEnable()
    {
        go.SetActive(Enabled.GetValue());
    }

    // Update is called once per frame
    void Update ()
    {
        if (AlwaysUpdate)
        {
            go.SetActive(Enabled.GetValue());
        }
	}
}
