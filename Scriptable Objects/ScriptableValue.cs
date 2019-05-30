using DragonDogStudios.Exceptions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static F;

public class ScriptableValue<T> : SerializedScriptableObject
{
    [OdinSerialize]
    protected bool _lock = false;
    public Option<T> DefaultValue;

    [OdinSerialize]
    private Option<T> _value;
    public Option<T> Value
    {
        set
        {
            if (_lock)
            {
                throw new ReadOnlyObjectEditException();
            }
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

    public static V CreateAsReadOnly<V>(V instance) where V : ScriptableValue<T>
    {
        instance._lock = true;
        return instance;
    }
}
