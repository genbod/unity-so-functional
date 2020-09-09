namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public interface ITransitionCondition
    {
        bool Evaluate();
        string ToString();
    }
}