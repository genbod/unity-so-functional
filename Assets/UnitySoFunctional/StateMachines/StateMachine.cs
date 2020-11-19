using System;
using System.Collections.Generic;
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

                if (_currentState != null)
                {
                    var currentStateString = _stateStack.Count > 0 ? _stateStack.Peek().Name : _currentState.Name;
                    currentStates.Add(currentStateString);

                    var currentState = _states[currentStateString].State;
                    if (currentState is IStateMachine)
                    {
                        var stateMachine = currentState as IStateMachine;
                        currentStates.AddRange(stateMachine.CurrentState);
                    }
                }
                
                return currentStates;
            }
        }
        

        public event Action<string> StateChanged;

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
                    StateChanged?.Invoke(_stateStack.Peek().Name);
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
            
            // No longer in first tick
            if (_firstTick) _firstTick = false;

            if (_stateStack.Count > 0)
            {
                _stateStack.Peek().Tick();
            }
            else _currentState?.Tick();
        }

        public void Exit()
        {
            if (_stateStack.Count > 0) Exit(_stateStack.Peek());
            Exit(_currentState);
        }

        internal void AddAnyTransition(IState to, ITransitionCondition transitionCondition)
        {
            var stateTransition = new StateTransition(null, to.Name, transitionCondition);
            _anyStateTransitions.Add(stateTransition);
        }

        internal void AddTransition(IState from, string to, ITransitionCondition transitionCondition)
        {
            var stateTransition = new StateTransition(from.Name, to, transitionCondition);
            _stateTransitions.Add(stateTransition);
        }

        internal void AddPushTransition(IState to, ITransitionCondition transitionCondition)
        {
            var stateTransition = new StateTransition(null, to.Name, transitionCondition);
            _pushTransitions.Add(stateTransition);
        }

        internal void AddPopTransition(IState from, ITransitionCondition transitionCondition)
        {
            var stateTransition = new StateTransition(from.Name, null, transitionCondition);
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
            
            if (_currentState != null) Exit(_currentState);

            _currentState = state;

            Enter(_currentState);
            Debug.Log($"Changed to state {state.Name}");
            StateChanged?.Invoke(_currentState.Name);
        }

        private StateConfiguration Configure(StateWrapper stateWrapper)
        {
            var stateConfiguration = new StateConfiguration(this, stateWrapper);
            return stateConfiguration;
        }

        private void Enter(IState state)
        {
            StateChanged?.Invoke(state.Name+ENTERED);
            state.OnEnter();
        }

        private void Exit(IState state)
        {
            state?.OnExit();
            StateChanged?.Invoke(state?.Name+EXITED);
            _firstTick = true;
        }

        private bool CheckForPopTransition(out StateTransition result)
        {
            foreach (var transition in _popTransitions)
            {
                if (_stateStack.Count > 0 &&
                    _states.TryGetValue(transition.From,
                        out var fromState) &&
                    fromState == _stateStack.Peek() &&
                    transition.ConditionMatches())
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
                if (transition.ConditionMatches())
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
                if (transition.ConditionMatches())
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
                    transition.ConditionMatches())
                {
                    result = transition;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public string ToDOT(List<string> outputList = null)
        {
            var output = outputList ?? new List<string>();

            foreach (var transition in _anyStateTransitions)
            {
                var entry = ($"Any -> \"{transition.To}\" [label=\"{transition.Expression}\"];");
                if (!output.Contains(entry)) output.Add(entry);
            }
            foreach (var transition in _stateTransitions)
            {
                var entry = ($"\"{transition.From}\" -> \"{transition.To}\" [label=\"{transition.Expression}\"];");
                if (!output.Contains(entry)) output.Add(entry);
            }
            
            // print sub graphs
            foreach (var state in _states)
            {
                if (state.Value.State is IStateMachine stateMachine)
                {
                    var entry = ($"subgraph \"cluster_{state.Key}\" {{");
                    if (output.Contains(entry)) continue;

                    output.Add(entry);
                    output.Add("style=filled");
                    stateMachine.ToDOT(output);
                    output.Add("}");
                }
            }
            
            return string.Join("\n", output);
        }
    }
}