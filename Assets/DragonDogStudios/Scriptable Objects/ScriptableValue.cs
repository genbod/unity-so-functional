using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static F;

public class ScriptableValue<T> : SerializedScriptableObject
{
    public Option<T> DefaultValue;

    public Option<T> Value;
    
    public void SetValue(Option<T> newValue)
        => Value = newValue;

    private void OnEnable()
    {
        Value = DefaultValue;
    }
}
