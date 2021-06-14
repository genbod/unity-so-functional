namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    internal class StateMachineWrapper : State, IState
    {
        private readonly IStateMachine _stateMachine;

        internal IStateMachine StateMachine => _stateMachine;

        internal StateMachineWrapper(
            string name,
            IStateMachine stateMachine) : base(name)
        {
            _stateMachine = stateMachine;
        }

        public new void Tick()
        {
            _stateMachine.Tick();
            base.Tick();
        }

        public new void OnEnter()
        {
            _stateMachine.Enter();
            base.OnEnter();
        }

        public new void OnExit()
        {
            _stateMachine.Exit();
            base.OnExit();
        }
    }
}