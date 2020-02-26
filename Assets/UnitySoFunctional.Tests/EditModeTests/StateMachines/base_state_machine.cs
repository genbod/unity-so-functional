using DragonDogStudios.UnitySoFunctional.StateMachines;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class base_state_machine
    {
        [Test]
        public void initial_set_state_switches_to_state()
        {
            var stateMachine = new StateMachine();
            IState firstState = Substitute.For<IState>();
            stateMachine.SetState(firstState);
            
            Assert.AreSame(firstState, stateMachine.CurrentState);
        }
        
        [Test]
        public void subsequent_set_state_switches_to_state()
        {
            var stateMachine = new StateMachine();
            IState firstState = Substitute.For<IState>();
            IState secondState = Substitute.For<IState>();
            
            stateMachine.SetState(firstState);
            Assert.AreSame(firstState, stateMachine.CurrentState);
            
            stateMachine.SetState(secondState);
            Assert.AreSame(secondState, stateMachine.CurrentState);
        }
        
        [Test]
        public void transition_switches_state_when_condition_is_met()
        {
            var stateMachine = new StateMachine();
            IState firstState = Substitute.For<IState>();
            IState secondState = Substitute.For<IState>();

            bool ShouldTransitionState() => true;
            stateMachine.AddTransition(firstState, secondState, ShouldTransitionState);
            
            stateMachine.SetState(firstState);
            Assert.AreSame(firstState, stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame(secondState, stateMachine.CurrentState);
        }

        [Test]
        public void transition_does_not_switch_state_when_condition_is_not_met()
        {
            var stateMachine = new StateMachine();
            IState firstState = Substitute.For<IState>();
            IState secondState = Substitute.For<IState>();

            bool ShouldTransitionState() => false;
            stateMachine.AddTransition(firstState, secondState, ShouldTransitionState);
            
            stateMachine.SetState(firstState);
            Assert.AreSame(firstState, stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame(firstState, stateMachine.CurrentState);
        }
        
        [Test]
        public void transition_does_not_switch_state_when_not_in_correct_source_state()
        {
            var stateMachine = new StateMachine();
            IState firstState = Substitute.For<IState>();
            IState secondState = Substitute.For<IState>();
            IState thirdState = Substitute.For<IState>();

            bool ShouldTransitionState() => true;
            stateMachine.AddTransition(secondState, thirdState, ShouldTransitionState);
            
            stateMachine.SetState(firstState);
            Assert.AreSame(firstState, stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame(firstState, stateMachine.CurrentState);
        }
        
        [Test]
        public void transition_from_any_switches_state_when_condition_is_met()
        {
            var stateMachine = new StateMachine();
            IState firstState = Substitute.For<IState>();
            IState secondState = Substitute.For<IState>();

            bool ShouldTransitionState() => true;
            stateMachine.AddAnyTransition(secondState, ShouldTransitionState);
            
            stateMachine.SetState(firstState);
            Assert.AreSame(firstState, stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame(secondState, stateMachine.CurrentState);
        }
    }
}
