using System;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnabledSetter : SerializedMonoBehaviour
{
    public bool AlwaysUpdate;

    public bool InvertValue;

    public Func<bool> BooleanGetter;

    public GameObject go;

    void Awake()
    {
        // During Awake, GameObject needs to be set active for any Awake setup that needs to happen before everything starts
        if (go != null)
        {
            go.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AlwaysUpdate)
        {
            //var newEnabled = Enabled.GetValue() ^ InvertValue;
            var newEnabled = BooleanGetter() ^ InvertValue;
            if (go.activeSelf != newEnabled)
            {
                go.SetActive(newEnabled);
            }
        }
    }
}
