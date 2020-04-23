using System;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
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