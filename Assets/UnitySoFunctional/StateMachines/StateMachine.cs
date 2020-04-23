using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public class StateMachine
    {
        public string CurrentState => _currentState.Name;
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

        private StateConfiguration Configure(StateWrapper stateWrapper)
        {
            var stateConfiguration = new StateConfiguration(this, stateWrapper);
            return stateConfiguration;
        }

        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            var stateTransition = new StateTransition(null, to.Name, condition);
            _anyStateTransitions.Add(stateTransition);
        }

        internal void AddTransition(IState from, string to, Func<bool> condition)
        {
            var stateTransition = new StateTransition(from.Name, to, condition);
            _stateTransitions.Add(stateTransition);
        }

        public void SetState(string stateName)
        {
            var state = _states[stateName];
            SetState(state);
        }

        private void SetState(IState state)
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
                if (_states.TryGetValue(transition.From,
                        out var fromState) &&
                    fromState == _currentState &&
                    transition.Condition())
                {
                    return transition;
                }
            }

            return null;
        }
    }
}