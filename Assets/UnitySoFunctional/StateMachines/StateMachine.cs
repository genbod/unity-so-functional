using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public class StateMachine
    {
        public IState CurrentState => _currentState;
        public event Action<IState> OnStateChanged;

        private Dictionary<string, StateWrapper> _states = new Dictionary<string, StateWrapper>();
        private List<StateTransition> _stateTransitions = new List<StateTransition>();
        private List<StateTransition> _anyStateTransitions = new List<StateTransition>();
        private IState _currentState;

        public StateConfiguration Configure(string stateName)
        {
            if (_states.ContainsKey(stateName))
            {
                return Configure(_states[stateName]);
            }
            var newState = new State(stateName);
            return Configure(newState);
        }

        public StateConfiguration Configure(IState state)
        {
            if (_states.ContainsKey(state.Name))
            {
                return Configure(_states[state.Name]);
            }
            var stateWrapper = new StateWrapper(state);
            _states.Add(state.Name, stateWrapper);
            return Configure(stateWrapper);
        }

        internal StateConfiguration Configure(StateWrapper stateWrapper)
        {
            var stateConfiguration = new StateConfiguration(this, stateWrapper);
            return stateConfiguration;
        }

        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            var stateTransition = new StateTransition(null, to, condition);
            _anyStateTransitions.Add(stateTransition);
        }

        internal void AddTransition(IState from, string to, Func<bool> condition)
        {
            var toState = _states[to];
            AddTransition(from, toState, condition);
        }
        
        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            var stateTransition = new StateTransition(from, to, condition);
            _stateTransitions.Add(stateTransition);
        }

        public void SetState(string stateName)
        {
            var state = _states[stateName];
            SetState(state);
        }

        public void SetState(IState state)
        {
            if (_currentState == state) return;

            _currentState?.OnExit();

            _currentState = state;
            Debug.Log($"Changed to state {state.Name}");
            _currentState.OnEnter();
            
            OnStateChanged?.Invoke(_currentState);
        }

        public void Tick()
        {
            StateTransition transition = CheckForTransition();
            if (transition != null)
            {
                SetState(transition.To);
            }

            _currentState.Tick();
        }

        private StateTransition CheckForTransition()
        {
            foreach (var transition in _anyStateTransitions)
            {
                if (transition.Condition())
                {
                    return transition;
                }
            }

            foreach (var transition in _stateTransitions)
            {
                if (transition.From == _currentState && transition.Condition())
                {
                    return transition;
                }
            }

            return null;
        }
    }

    internal class StateWrapper : IState
    {
        private IState _state;
        private List<Action> _enterActions = new List<Action>();
        private List<Action> _exitActions = new List<Action>();

        internal StateWrapper(IState state)
        {
            _state = state;
        }

        internal void AddEnterAction(Action enterAction)
        {
            _enterActions.Add(enterAction);
        }

        internal void AddExitAction(Action exitAction)
        {
            _exitActions.Add(exitAction);
        }

        public string Name => _state.Name;
        
        public void Tick()
        {
            _state?.Tick();
        }

        public void OnEnter()
        {
            _state?.OnEnter();
            foreach (var enterAction in _enterActions)
            {
                enterAction.Invoke();
            }
        }

        public void OnExit()
        {
            _state?.OnExit();
            foreach (var exitAction in _exitActions)
            {
                exitAction.Invoke();
            }
        }
    }

    public class StateConfiguration
    {
        private StateMachine _stateMachine;
        private StateWrapper _stateWrapper;

        internal StateConfiguration(StateMachine stateMachine, StateWrapper state)
        {
            _stateMachine = stateMachine;
            _stateWrapper = state;
        }

        public StateConfiguration OnEnter(Action enterAction)
        {
            _stateWrapper.AddEnterAction(enterAction);
            return this;
        }

        public StateConfiguration OnExit(Action exitAction)
        {
            _stateWrapper.AddExitAction(exitAction);
            return this;
        }

        public StateConfiguration Transition(string state, Func<bool> condition)
        {
            _stateMachine.AddTransition(_stateWrapper, state, condition);
            return this;
        }

        public StateConfiguration AnyTransition(Func<bool> condition)
        {
            _stateMachine.AddAnyTransition(_stateWrapper, condition);
            return this;
        }
    }
}