using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class TransitionConfiguration : SerializedScriptableObject
    {
        public static TransitionConfiguration Create(
            string transitionCondition,
            ObjectIdentifier stateID)
        {
            var transitionConfiguration = CreateInstance<TransitionConfiguration>();
            transitionConfiguration._stateID = stateID;
            transitionConfiguration._condition = transitionCondition;
            return transitionConfiguration;
        }
        public static TransitionConfiguration Create(
            string transitionCondition,
            string state = null)
        {
            var transitionConfiguration = CreateInstance<TransitionConfiguration>();
            transitionConfiguration._toStateName = state;
            transitionConfiguration._condition = transitionCondition;
            return transitionConfiguration;
        }
        
        [SerializeField] private string _toStateName;
        [SerializeField] string _condition;
        [SerializeField] private ObjectIdentifier _stateID;

        public string ToStateName => _toStateName;
        public string Condition => _condition;
        public ObjectIdentifier StateID => _stateID;

        public void SetStateName(string stateName)
        {
            _toStateName = stateName;
        }
    }
}