using System;
using System.Collections.Generic;
using DragonDogStudios.UnitySoFunctional.StateMachines.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class TransitionConfiguration : SerializedScriptableObject
    {
        public static TransitionConfiguration Create(
            string transitionCondition,
            Guid owningStateID,
            Guid? stateID)
        {
            var transitionConfiguration = CreateInstance<TransitionConfiguration>();
            transitionConfiguration._owningStateID = owningStateID;
            transitionConfiguration._toStateID = stateID;
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

        private static List<string> _triggerNames;
        
        [SerializeField, ShowIf("_toStateName"), ReadOnly] private string _toStateName;
        [SerializeField, ValueDropdown("GetTriggerNames")] string _condition;
        [HideInInspector, SerializeField] private Guid _owningStateID;
        [HideInInspector, SerializeField] private Guid? _toStateID;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Triangle _arrow;

        public string ToStateName => _toStateName;
        public string Condition => _condition;
        public Guid? ToStateID => _toStateID;

        public Vector2 StartPosition => _startPosition;
        public Vector2 EndPosition => _endPosition;
        public Triangle Arrow => _arrow;
        public Guid OwningStateID => _owningStateID;

        public void SetToStateName(string stateName)
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
#if UNITY_EDITOR
        private static IEnumerable<string> GetTriggerNames()
        {
            if (_triggerNames == null)
            {
                UpdateTriggerNames();
            }

            return _triggerNames;
        }

        [Button]
        public static void UpdateTriggerNames()
        {
            _triggerNames = new List<string>();
            _triggerNames.AddRange(
                EditorHelpers
                    .GetFieldsFromInterfaceImplementations(typeof(ITriggerNames)));
        }
#endif
    }
}