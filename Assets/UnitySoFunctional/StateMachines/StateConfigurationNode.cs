using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class TransitionConfiguration
    {
        [SerializeField] private string _state;
        [SerializeField] string _condition;

        public string State => _state;
        public string Condition => _condition;

        public TransitionConfiguration(string transitionCondition, string state = null)
        {
            _state = state;
            _condition = transitionCondition;
        }
    }
    
    [Serializable]
    public class StateConfigurationNode : ScriptableObject
    {
        [SerializeField] private Rect _rect = new Rect(0, 0, 200, 100);
        [SerializeField] private List<TransitionConfiguration> _transitions = new List<TransitionConfiguration>();
        [SerializeField] private List<TransitionConfiguration> _anyTransitions = new List<TransitionConfiguration>();
        [SerializeField] private List<TransitionConfiguration> _pushTransition = new List<TransitionConfiguration>();
        [SerializeField] private List<TransitionConfiguration> _popTransition = new List<TransitionConfiguration>();
        
        private readonly UnityEvent _onEnter = new UnityEvent();
        private readonly UnityEvent _onExit = new UnityEvent();
        private readonly UnityEvent _onTick = new UnityEvent();

        public Rect Rect => _rect;

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

        public StateConfigurationNode Transition(string state, string transitionCondition)
        {
            _transitions.Add(
                new TransitionConfiguration(transitionCondition, state));
            return this;
        }

        public StateConfigurationNode AnyTransition(string transitionCondition)
        {
            _anyTransitions.Add(
                new TransitionConfiguration(transitionCondition));
            return this;
        }

        public StateConfigurationNode PushTransition(string transitionCondition)
        {
            _pushTransition.Add(
                new TransitionConfiguration(transitionCondition));
            return this;
        }

        public StateConfigurationNode PopTransition(string transitionCondition)
        {
            _popTransition.Add(
                new TransitionConfiguration(transitionCondition));
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
                    transition.State,
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
        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move State Node");
            _rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}