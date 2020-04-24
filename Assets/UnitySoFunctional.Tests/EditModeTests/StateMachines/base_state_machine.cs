using System;
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
            string stateName = "firstState";
            stateMachine.Configure(stateName);
            stateMachine.SetState(stateName);
            
            Assert.AreSame(stateName, stateMachine.CurrentState);
        }
        
        [Test]
        public void subsequent_set_state_switches_to_state()
        {
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState");
            stateMachine.Configure("secondState");
            
            stateMachine.SetState("firstState");
            Assert.AreSame("firstState", stateMachine.CurrentState);
            
            stateMachine.SetState("secondState");
            Assert.AreSame("secondState", stateMachine.CurrentState);
        }
        
        [Test]
        public void transition_switches_state_when_condition_is_met()
        {
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState")
                .Transition("secondState", ShouldTransitionState);
            stateMachine.Configure("secondState");
            bool ShouldTransitionState() => true;

            stateMachine.SetState("firstState");
            Assert.AreSame("firstState", stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame("secondState", stateMachine.CurrentState);
        }

        [Test]
        public void transition_does_not_switch_state_when_condition_is_not_met()
        {
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState")
                .Transition("secondState", ShouldTransitionState);
            stateMachine.Configure("secondState");
            bool ShouldTransitionState() => false;

            stateMachine.SetState("firstState");
            Assert.AreSame("firstState", stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame("firstState", stateMachine.CurrentState);
        }
        
        [Test]
        public void transition_does_not_switch_state_when_not_in_correct_source_state()
        {
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState");
            stateMachine.Configure("secondState")
                .Transition("thirdState", ShouldTransitionState);
            stateMachine.Configure("thirdState");

            bool ShouldTransitionState() => true;

            stateMachine.SetState("firstState");
            Assert.AreSame("firstState", stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame("firstState", stateMachine.CurrentState);
        }
        
        [Test]
        public void transition_from_any_switches_state_when_condition_is_met()
        {
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState");
            stateMachine.Configure("secondState")
                .AnyTransition(ShouldTransitionState);
            bool ShouldTransitionState() => true;
            
            stateMachine.SetState("firstState");
            Assert.AreSame("firstState", stateMachine.CurrentState);
            
            stateMachine.Tick();
            
            Assert.AreSame("secondState", stateMachine.CurrentState);
        }

        [Test]
        public void pushTransition_from_any_state_switches_state_when_condition_is_met()
        {
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState");
            stateMachine.Configure("secondState")
                .PushTransition(ShouldTransition);
            bool ShouldTransition() => true;
            
            stateMachine.SetState("firstState");
            Assert.AreEqual("firstState", stateMachine.CurrentState);

            stateMachine.Tick();
            
            Assert.AreEqual("secondState", stateMachine.CurrentState);
        }
        
        [Test]
        public void popTransition_goes_back_to_original_state_when_conditions_met()
        {
            var ShouldPushTransition = Substitute.For<Func<bool>>();
            ShouldPushTransition().Returns(x => true, x=> false);
            bool ShouldTransition() => true;
            
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState");
            stateMachine.Configure("secondState")
                .PushTransition(ShouldPushTransition)
                .PopTransition(ShouldTransition);

            stateMachine.SetState("firstState");
            Assert.AreEqual("firstState", stateMachine.CurrentState);

            stateMachine.Tick();
            
            Assert.AreEqual("secondState", stateMachine.CurrentState);

            stateMachine.Tick();
            
            Assert.AreEqual("firstState", stateMachine.CurrentState);
        }
        
        [Test]
        public void popTransition_does_not_go_back_to_original_state_if_not_in_correct_push_state()
        {
            var ShouldPushTransition = Substitute.For<Func<bool>>();
            ShouldPushTransition().Returns(x => true, x=> false);
            bool ShouldTransition() => true;
            
            var stateMachine = new StateMachine();
            stateMachine.Configure("firstState")
                .PopTransition(ShouldTransition);
            stateMachine.Configure("secondState")
                .PushTransition(ShouldPushTransition);

            stateMachine.SetState("firstState");
            Assert.AreEqual("firstState", stateMachine.CurrentState);

            stateMachine.Tick();
            
            Assert.AreEqual("secondState", stateMachine.CurrentState);

            stateMachine.Tick();
            
            Assert.AreEqual("secondState", stateMachine.CurrentState);
        }
    }
}
