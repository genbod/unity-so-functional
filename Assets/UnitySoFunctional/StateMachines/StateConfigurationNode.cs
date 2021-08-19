using System;
using System.Collections.Generic;
using System.Linq;
using DragonDogStudios.UnitySoFunctional.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class StateConfigurationNode : SerializedScriptableObject
    {
        [HideInInspector, SerializeField] private string _id = Guid.NewGuid().ToString();
        [SerializeField, ValueDropdown("GetStateNames")]
        private string _name;

        [HideInInspector, SerializeField] private Vector2 _pos;
        [SerializeField] private List<TransitionConfiguration> _transitions = new List<TransitionConfiguration>();
        [SerializeField] private List<TransitionConfiguration> _anyTransitions = new List<TransitionConfiguration>();
        [SerializeField] private List<TransitionConfiguration> _pushTransition = new List<TransitionConfiguration>();
        [SerializeField] private List<TransitionConfiguration> _popTransition = new List<TransitionConfiguration>();

        private Rect _rect = new Rect(0, 0, 200, 50);
        private readonly UnityEvent _onEnter = new UnityEvent();
        private readonly UnityEvent _onExit = new UnityEvent();

        private readonly UnityEvent _onTick = new UnityEvent();

        private static List<string> _stateNames;

        public Rect Rect
        {
            get
            {
                _rect.position = _pos;
                return _rect;
            }
        }

        public IEnumerable<TransitionConfiguration> Transitions
        {
            get
            {
                return _transitions;
            }
        }

        public Guid ID => Guid.Parse(_id);

        public StateConfigurationNode OnEnter(UnityAction enterAction)
        {
            _onEnter.AddListener(enterAction);
            return this;
        }

        public StateConfigurationNode OnExit(UnityAction exitAction)
        {
            _onExit.AddListener(exitAction);
            return this;
        }

        public StateConfigurationNode OnTick(UnityAction tickAction)
        {
            _onTick.AddListener(tickAction);
            return this;
        }

        public StateConfigurationNode Transition(string stateName, string transitionCondition)
        {
            var transitionConfiguration = TransitionConfiguration.Create(
                transitionCondition,
                stateName);
            _transitions.Add(transitionConfiguration);
            return this;
        }

        public StateConfigurationNode AnyTransition(string transitionCondition)
        {
            _anyTransitions.Add(
                TransitionConfiguration.Create(transitionCondition));
            return this;
        }

        public StateConfigurationNode PushTransition(string transitionCondition)
        {
            _pushTransition.Add(
                TransitionConfiguration.Create(transitionCondition));
            return this;
        }

        public StateConfigurationNode PopTransition(string transitionCondition)
        {
            _popTransition.Add(
                TransitionConfiguration.Create(transitionCondition));
            return this;
        }

        public void Build(
            StateMachine stateMachine,
            IState state,
            Func<string, ITransitionCondition> transitionConditionCreator)
        {
            state.SetTickActions(_onTick);
            state.SetOnEnterActions(_onEnter);
            state.SetOnExitActions(_onExit);
            foreach (var transition in _transitions)
            {
                stateMachine.AddTransition(
                    state,
                    transition.ToStateName,
                    transitionConditionCreator.Invoke(transition.Condition));
            }

            foreach (var transition in _anyTransitions)
            {
                stateMachine.AddAnyTransition(
                    state,
                    transitionConditionCreator.Invoke(transition.Condition));
            }

            foreach (var transition in _pushTransition)
            {
                stateMachine.AddPushTransition(
                    state,
                    transitionConditionCreator.Invoke(transition.Condition));
            }

            foreach (var transition in _popTransition)
            {
                stateMachine.AddPopTransition(
                    state,
                    transitionConditionCreator.Invoke(transition.Condition));
            }
        }

#if UNITY_EDITOR

        public void SetName(string newName)
        {
            _name = name = newName;
        }

        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move State Node");
            _pos = newPosition;
            EditorUtility.SetDirty(this);
        }

        public void AddTransition(Guid toStateID, string condition)
        {
            // Check to see if transition already exists
            if (_transitions.Exists(x => x.ToStateID == toStateID))
            {
                return;
            }
            Undo.RecordObject(this, "Added Transition");
            _transitions.Add(
                TransitionConfiguration.Create(
                    condition,
                    ID,
                    toStateID));
            Undo.RegisterCreatedObjectUndo(Transitions.Last(), "Created Transition");
        }

        public void DeleteTransition(TransitionConfiguration selectedTransition)
        {
            Undo.RecordObject(this, "Deleted Transition");
            _transitions.Remove(selectedTransition);
            Undo.DestroyObjectImmediate(selectedTransition);
        }

        private void OnValidate()
        {
            name = _name;
        }

        private static IEnumerable<string> GetStateNames()
        {
            if (_stateNames == null)
            {
                UpdateStateNames();
            }

            return _stateNames;
        }

        [Button]
        private static void UpdateStateNames()
        {
            _stateNames = new List<string>();
            _stateNames.AddRange(
                EditorHelpers
                    .GetFieldsFromInterfaceImplementations(typeof(IStateMachineNames)));
        }

        private string GetStateName(ObjectIdentifier assetID)
        {
            var asset = ObjectIdentifier.ToObject(assetID) as StateConfigurationNode;
            return asset?.name;
        }

#endif
    }
}