using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationParameterSetter<T> : SerializedMonoBehaviour
{
    [Tooltip("Variable to read from and send to the Animator as the specified parameter.")]
    public Func<T> ValueGetter;

    public Action<int, T> AnimationSetter;

    [Tooltip("Name of the parameter to set with the value of Variable")]
    public string ParameterName;

    [SerializeField]
    private int parameterHash;

	// Use this for initialization
	void Start ()
    {
        parameterHash = Animator.StringToHash(ParameterName);
	}
	
	// Update is called once per frame
	void Update ()
    {
        AnimationSetter(parameterHash, ValueGetter());	
	}
}
