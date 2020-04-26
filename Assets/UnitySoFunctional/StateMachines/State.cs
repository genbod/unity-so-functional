using System;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    internal class State : IState
    {
        public string Name => _name;
        
        private string _name;
        private Action _enterAction;
        private Action _tickAction;
        private Action _exitAction;

        internal State(string stateName)
        {
            _name = stateName;
        }

        public void Tick()
        {
            _tickAction?.Invoke();
        }

        public void OnEnter()
        {
            _enterAction?.Invoke();
        }

        public void OnExit()
        {
            _exitAction?.Invoke();
        }
    }
}