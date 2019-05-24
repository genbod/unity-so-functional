using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static F;

public class ScriptableValue<T> : SerializedScriptableObject
{
    public Option<T> DefaultValue;

    [OdinSerialize]
    private Option<T> _value;
    public Option<T> Value
    {
        private set
        {
            _value = value;
        }
        get
        {
            return _value;
        }
    }

    public void SetValue(Option<T> newValue)
        => Value = newValue;

    public void SetValue(T newValue)
        => Value = Some(newValue);

    private void OnEnable()
    {
        if (DefaultValue.IsSome())
        {
            Value = DefaultValue;
        }
    }

    public Option<System.Object> GetValueAsOption()
    {
        // This doesn't work for None becasue returned value for None is converted to Option<Option.None>
        return Value.Match(
            () => None,
            (f) => Some((System.Object)f)
        );
    }
}
