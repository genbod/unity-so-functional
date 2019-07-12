using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Unit = System.ValueTuple;

public class AnimationTriggerSetter : SerializedMonoBehaviour
{

    [SerializeField]
    private Func<string> ValueGetter;

    [Tooltip("SetTrigger Function from Animator you want to call")]
    public Action<int> AnimationSetter;

    private string _currentState;

    private void Start()
    {
        _currentState = ValueGetter();
        AnimationSetter(Animator.StringToHash(_currentState));
    }

    private void Update()
    {
        if (ValueGetter() != _currentState)
        {
            _currentState = ValueGetter();
            AnimationSetter(Animator.StringToHash(_currentState));
        }
    }
}