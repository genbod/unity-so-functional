using System;
using System.Linq.Expressions;

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

        public StateConfiguration Transition(string state, Expression<Func<bool>> condition)
        {
            _stateMachine.AddTransition(_stateWrapper, state, condition);
            return this;
        }

        public StateConfiguration AnyTransition(Expression<Func<bool>> condition)
        {
            _stateMachine.AddAnyTransition(_stateWrapper, condition);
            return this;
        }

        public StateConfiguration PushTransition(Expression<Func<bool>> condition)
        {
            _stateMachine.AddPushTransition(_stateWrapper, condition);
            return this;
        }

        public StateConfiguration PopTransition(Expression<Func<bool>> condition)
        {
            _stateMachine.AddPopTransition(_stateWrapper, condition);
            return this;
        }
    }
}