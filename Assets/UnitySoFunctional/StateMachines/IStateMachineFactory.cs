namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public interface IStateMachineFactory
    {
        IStateMachine Create(string stateName);
    }
}