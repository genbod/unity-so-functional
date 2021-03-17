using System;
using System.Collections.Generic;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    internal class StateWrapper : IState
    {
        private IState _state;
        private List<Action> _enterActions = new List<Action>();
        private List<Action> _exitActions = new List<Action>();
        private List<Action> _tickActions = new List<Action>();

        internal IState State => _state;

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

        public void AddTickAction(Action tickAction)
        {
            _tickActions.Add(tickAction);
        }

        public string Name => _state.Name;

        public void Tick()
        {
            _state?.Tick();
            foreach (var tickAction in _tickActions)
            {
                tickAction.Invoke();
            }
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
}