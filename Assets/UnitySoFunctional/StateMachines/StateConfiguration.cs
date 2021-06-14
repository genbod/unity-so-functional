using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    [Serializable]
    public class TransitionConfiguration
    {
        private string _state;
        private string _condition;

        public string State => _state;
        public string Condition => _condition;

        public TransitionConfiguration(string transitionCondition, string state = null)
        {
            _state = state;
            _condition = transitionCondition;
        }
    }
    
    [Serializable]
    public class StateConfiguration
    {
        private readonly string _stateName;
        private readonly UnityEvent _onEnter = new UnityEvent();
        private readonly UnityEvent _onExit = new UnityEvent();
        private readonly UnityEvent _onTick = new UnityEvent();
        private List<TransitionConfiguration> _transitions = new List<TransitionConfiguration>();
        private List<TransitionConfiguration> _anyTransitions = new List<TransitionConfiguration>();
        private List<TransitionConfiguration> _pushTransition = new List<TransitionConfiguration>();
        private List<TransitionConfiguration> _popTransition = new List<TransitionConfiguration>();

        public string Name => _stateName;

        public StateConfiguration(string stateName)
        {
            _stateName = stateName;
        }

        public StateConfiguration OnEnter(UnityAction enterAction)
        {
            _onEnter.AddListener(enterAction);
            return this;
        }

        public StateConfiguration OnExit(UnityAction exitAction)
        {
            _onExit.AddListener(exitAction);
            return this;
        }

        public StateConfiguration OnTick(UnityAction tickAction)
        {
            _onTick.AddListener(tickAction);
            return this;
        }

        public StateConfiguration Transition(string state, string transitionCondition)
        {
            _transitions.Add(
                new TransitionConfiguration(transitionCondition, state));
            return this;
        }

        public StateConfiguration AnyTransition(string transitionCondition)
        {
            _anyTransitions.Add(
                new TransitionConfiguration(transitionCondition));
            return this;
        }

        public StateConfiguration PushTransition(string transitionCondition)
        {
            _pushTransition.Add(
                new TransitionConfiguration(transitionCondition));
            return this;
        }

        public StateConfiguration PopTransition(string transitionCondition)
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
    }
}