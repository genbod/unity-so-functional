using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class TransitionConfiguration : SerializedScriptableObject
    {
        public static TransitionConfiguration Create(
            string transitionCondition,
            string state = null)
        {
            var transitionConfiguration = CreateInstance<TransitionConfiguration>();
            transitionConfiguration._state = state;
            transitionConfiguration._condition = transitionCondition;
            return transitionConfiguration;
        }
        
        [SerializeField] private string _state;
        [SerializeField] string _condition;

        public string State => _state;
        public string Condition => _condition;
    }
}