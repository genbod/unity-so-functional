namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public interface IState
    {
        string Name { get; }
        void Tick();
        void OnEnter();
        void OnExit();
    }
}