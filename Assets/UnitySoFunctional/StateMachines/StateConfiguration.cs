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

        public StateConfiguration Transition(string state, ITransitionCondition transitionCondition)
        {
            _stateMachine.AddTransition(_stateWrapper, state, transitionCondition);
            return this;
        }

        public StateConfiguration AnyTransition(ITransitionCondition transitionCondition)
        {
            _stateMachine.AddAnyTransition(_stateWrapper, transitionCondition);
            return this;
        }

        public StateConfiguration PushTransition(ITransitionCondition transitionCondition)
        {
            _stateMachine.AddPushTransition(_stateWrapper, transitionCondition);
            return this;
        }

        public StateConfiguration PopTransition(ITransitionCondition transitionCondition)
        {
            _stateMachine.AddPopTransition(_stateWrapper, transitionCondition);
            return this;
        }
    }
}