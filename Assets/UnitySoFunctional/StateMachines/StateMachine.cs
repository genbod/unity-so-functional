using System;
using System.Collections.Generic;
using System.Linq;
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

                    var currentState = _states[currentStateString];
                    if (currentState is StateMachineWrapper stateMachineWrapper)
                    {
                        currentStates.AddRange(
                            stateMachineWrapper
                            .StateMachine
                            .CurrentState);
                    }
                }
                
                return currentStates;
            }
        }

        public string LocalCurrentStateName => _currentState?.Name;
        public List<string> StateStackNames => _stateStack.Select(x => x.Name).Reverse().ToList();
        
        public event Action<string> StateChanged;

        public const string ENTERED = ".entered";
        public const string EXITED = ".exited";

        private Dictionary<string, IState> _states = new Dictionary<string, IState>();
        private List<IStateMachine> _subStateMachines = new List<IStateMachine>();
        private List<StateTransition> _stateTransitions = new List<StateTransition>();
        private List<StateTransition> _anyStateTransitions = new List<StateTransition>();
        private List<StateTransition> _pushTransitions = new List<StateTransition>();
        private List<StateTransition> _popTransitions = new List<StateTransition>();
        private IState _currentState;
        private Stack<IState> _stateStack = new Stack<IState>();
        private string _startState;
        private bool _firstTick = true;

        public void Dispose()
        {
            foreach (var stateMachine in _subStateMachines)
            {
                stateMachine.StateChanged -= FireStateChanged;
                stateMachine.Dispose();
            }
        }

        public IState GetState(string stateName)
        {
            if (_states.ContainsKey(stateName))
            {
                return _states[stateName];
            }

            var newState = new State(stateName);
            _states.Add(stateName, newState);
            return newState;
        }

        public IState GetSubStateMachine(
            string stateName,
            IStateMachineFactory stateMachineFactory)
        {
            if (_states.ContainsKey(stateName))
            {
                return _states[stateName];
            }

            var stateMachine = stateMachineFactory.Create(stateName);
            RegisterStateMachine(stateMachine);
            var stateMachineWrapper = new StateMachineWrapper(
                stateName,
                stateMachine);
            _states.Add(stateName,  stateMachineWrapper);
            return stateMachineWrapper;
        }

        public void SetStartState(string startState)
        {
            _startState = startState;
        }

        public void FireStateChanged(string state)
        {
            StateChanged?.Invoke(state);
        }

        public void Tick()
        {
            StateTransition transition;
            if (_firstTick && !String.IsNullOrEmpty(_startState))
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

        public void Enter()
        {
            // Nothing to do here but other implementations of IStateMachine do
        }

        public void Exit()
        {
            if (_stateStack.Count > 0) Exit(_stateStack.Peek());
            Exit(_currentState);
        }

        public void LoadSavedState(string currentState)
        {
            SetState(currentState, false);
            _firstTick = false;
        }

        public void LoadStateStack(List<string> stackStateNames)
        {
            _stateStack.Clear();
            foreach (var stateName in stackStateNames)
            {
                if (_states.TryGetValue(stateName, out var stateMachine))
                {
                    _stateStack.Push(stateMachine);
                }
            }
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

        private void SetState(string stateName, bool withTransitions = true)
        {
            if (!_states.TryGetValue(stateName, out var state))
            {
                Debug.LogError($"State name: {stateName} does not exist");
                return;
            }

            var oldState = _currentState;
            _currentState = state;

            if (!withTransitions)
            {
                Debug.Log($"Set state to: {state.Name}");
                return;
            }
            if (oldState != null) Exit(oldState);
            
            Enter(_currentState);
            Debug.Log($"Changed to state: {state.Name}");
            StateChanged?.Invoke(_currentState.Name);
        }

        private void RegisterStateMachine(IStateMachine subStateMachine)
        {
            subStateMachine.StateChanged += FireStateChanged;
            _subStateMachines.Add(subStateMachine);
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
                if (state.Value is StateMachineWrapper stateMachineWrapper)
                {
                    var entry = ($"subgraph \"cluster_{state.Key}\" {{");
                    if (output.Contains(entry)) continue;

                    output.Add(entry);
                    output.Add("style=filled");
                    stateMachineWrapper.StateMachine.ToDOT(output);
                    output.Add("}");
                }
            }
            
            return string.Join("\n", output);
        }
    }
}