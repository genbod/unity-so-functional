using System;
using DragonDogStudios.UnitySoFunctional.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class TransitionConfiguration : SerializedScriptableObject
    {
        public static TransitionConfiguration Create(
            string transitionCondition,
            Guid stateID)
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
        [SerializeField] private Guid _stateID;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Triangle _arrow;

        public string ToStateName => _toStateName;
        public string Condition => _condition;
        public Guid StateID => _stateID;

        public Vector2 StartPosition => _startPosition;
        public Vector2 EndPosition => _endPosition;
        public Triangle Arrow => _arrow;

        public void SetStateName(string stateName)
        {
            _toStateName = stateName;
        }

        public void SetStartPosition(Vector2 startPosition)
        {
            _startPosition = startPosition;
        }

        public void SetEndPosition(Vector2 endPosition)
        {
            _endPosition = endPosition;
        }

        public void SetArrow(Triangle triangle)
        {
            _arrow = triangle;
        }
    }
}