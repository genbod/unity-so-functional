using System;
using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    internal class State : IState
    {
        public string Name => _name;
        
        private string _name;
        private UnityEvent _enterAction;
        private UnityEvent _tickAction;
        private UnityEvent _exitAction;

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

        public void SetTickActions(UnityEvent onTick)
        {
            _tickAction = onTick;
        }

        public void SetOnEnterActions(UnityEvent onEnter)
        {
            _enterAction = onEnter;
        }

        public void SetOnExitActions(UnityEvent onExit)
        {
            _exitAction = onExit;
        }
    }
}