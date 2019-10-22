using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringVariableChangedEventListener : ValueChangedEventListener<string>
{
    [SerializeField]
    private StringVariable _stringVariable;

    private void Awake()
    {
        this.Event = _stringVariable.ValueChangedEvent;
    }
}
