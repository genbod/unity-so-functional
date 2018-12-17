using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextReplacer : SerializedMonoBehaviour { 
    public Func<bool, string> GetValueToString;

    public Text Text;

    public bool PrettyPrint;

    public bool AlwaysUpdate;

    private void OnEnable()
    {
        if (Text != null && GetValueToString != null)
        {
            Text.text = GetValueToString(PrettyPrint);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AlwaysUpdate && Text != null && GetValueToString != null)
        {
            Text.text = GetValueToString(PrettyPrint);
        }
    }
}
