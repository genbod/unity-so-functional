﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public class StateMachine : IStateMachine
    {
        public IReadOnlyList<string> CurrentState
        {
            get
            {
                List<string> currentStates = new List<string>();
                var currentStateString = _stateStack.Count > 0 ? _stateStack.Peek().Name : _currentState.Name;
                currentStates.Add(currentStateString);

                var currentState = _states[currentStateString].State;
                if (currentState is IStateMachine)
                {
                    var stateMachine = currentState as IStateMachine;
                    currentStates.AddRange(stateMachine.CurrentState);
                }
                return currentStates;
            }
        }
        

        public event Action<string> OnStateChanged;

        public const string ENTERED = ".entered";
        public const string EXITED = ".exited";
        
        private Dictionary<string, StateWrapper> _states = new Dictionary<string, StateWrapper>();
        private List<StateTransition> _stateTransitions = new List<StateTransition>();
        private List<StateTransition> _anyStateTransitions = new List<StateTransition>();
        private List<StateTransition> _pushTransitions = new List<StateTransition>();
        private List<StateTransition> _popTransitions = new List<StateTransition>();
        private IState _currentState;
        private Stack<IState> _stateStack = new Stack<IState>();
        private string _startState;
        private bool _firstTick = true;

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

        public void SetStartState(string startState)
        {
            _startState = startState;
        }

        public void Tick()
        {
            StateTransition transition;
            if (_firstTick && _startState != null)
            {
                SetState(_startState);
            }
            else if (CheckForPushTransition(out transition))
            {
                var state = _states[transition.To];
                if (state != null)
                {
                    _stateStack.Push(state);
                    Debug.Log($"Pushed state {state.Name}");
                    Enter(_stateStack.Peek());
                    OnStateChanged?.Invoke(_stateStack.Peek().Name);
                }
            }
            else if (CheckForPopTransition(out transition))
            {
                if (_stateStack.Count > 0)
                {
                    Exit(_stateStack.Peek());
                    Debug.Log($"Popped State {_stateStack.Peek().Name}");
                    _stateStack.Pop();
                }
            }
            else if(CheckForTransition(out transition))
            {
                SetState(transition.To);
            }
            
            if (_firstTick) _firstTick = false;

            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Tick();
            }
            else _currentState.Tick();
        }

        public void Exit()
        {
            if (_stateStack.Count > 0) Exit(_stateStack.Peek());
            Exit(_currentState);
        }

        internal void AddAnyTransition(IState to, Expression<Func<bool>> condition)
        {
            var stateTransition = new StateTransition(null, to.Name, condition);
            _anyStateTransitions.Add(stateTransition);
        }

        internal void AddTransition(IState from, string to, Expression<Func<bool>> condition)
        {
            var stateTransition = new StateTransition(from.Name, to, condition);
            _stateTransitions.Add(stateTransition);
        }

        internal void AddPushTransition(IState to, Expression<Func<bool>> condition)
        {
            var stateTransition = new StateTransition(null, to.Name, condition);
            _pushTransitions.Add(stateTransition);
        }

        internal void AddPopTransition(IState from, Expression<Func<bool>> condition)
        {
            var stateTransition = new StateTransition(from.Name, null, condition);
            _popTransitions.Add(stateTransition);
        }

        private void SetState(string stateName)
        {
            StateWrapper state;
            if (!_states.TryGetValue(stateName, out state))
            {
                Debug.LogError($"State name: {stateName} does not exist");
                return;
            }
            if (_currentState == state) return;

            if (_currentState != null) Exit(_currentState);

            _currentState = state;

            Enter(_currentState);
            Debug.Log($"Changed to state {state.Name}");
            OnStateChanged?.Invoke(_currentState.Name);
        }

        private StateConfiguration Configure(StateWrapper stateWrapper)
        {
            var stateConfiguration = new StateConfiguration(this, stateWrapper);
            return stateConfiguration;
        }

        private void Enter(IState state)
        {
            OnStateChanged?.Invoke(state.Name+ENTERED);
            state.OnEnter();
        }

        private void Exit(IState state)
        {
            state?.OnExit();
            OnStateChanged?.Invoke(state?.Name+EXITED);
        }

        private bool CheckForPopTransition(out StateTransition result)
        {
            foreach (var transition in _popTransitions)
            {
                if (_stateStack.Count > 0 &&
                    _states.TryGetValue(transition.From,
                        out var fromState) &&
                    fromState == _stateStack.Peek() &&
                    transition.Condition())
                {
                    result = transition;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool CheckForPushTransition(out StateTransition result)
        {
            foreach (var transition in _pushTransitions)
            {
                if (transition.Condition())
                {
                    result = transition;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool CheckForTransition(out StateTransition result)
        {
            foreach (var transition in _anyStateTransitions)
            {
                if (transition.Condition())
                {
                    result = transition;
                    return true;
                }
            }

            foreach (var transition in _stateTransitions)
            {
                if (_states.TryGetValue(transition.From,
                        out var fromState) &&
                    fromState == _currentState &&
                    transition.Condition())
                {
                    result = transition;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public string ToDOT()
        {
            StringBuilder dot = new StringBuilder();
            
            foreach (var transition in _anyStateTransitions)
            {
                dot.AppendLine($"Any -> \"{transition.To}\" [label=\"{transition.Expression}\"];");
            }
            foreach (var transition in _stateTransitions)
            {
                dot.AppendLine($"\"{transition.From}\" -> \"{transition.To}\" [label=\"{transition.Expression}\"];");
            }
            
            // print sub graphs
            foreach (var state in _states)
            {
                if (state.Value.State is IStateMachine stateMachine)
                {
                    dot.AppendLine($"subgraph \"cluster_{state.Key}\" {{");
                    dot.AppendLine("style=filled");
                    dot.Append(stateMachine.ToDOT());
                    dot.AppendLine("}");
                }
            }
            
            return dot.ToString();
        }
    }
}