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

    [SerializeField]
    private Animator _animator;

    private string _currentState;

    private void Start()
    {
        _currentState = ValueGetter();
        if (_animator != null)
        {
            _animator.SetTrigger(Animator.StringToHash(_currentState));
        }
    }

    private void Update()
    {
        var newState = ValueGetter();
        if (newState != _currentState && !_animator.GetBool(Animator.StringToHash(newState)))
        {
            _currentState = ValueGetter();
            _animator.SetTrigger(Animator.StringToHash(_currentState));
        }
    }
}