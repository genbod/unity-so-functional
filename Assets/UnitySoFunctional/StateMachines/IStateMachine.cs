using System;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public interface IStateMachine
    {
        event Action<string> OnStateChanged;
        string ToDOT();
    }
}