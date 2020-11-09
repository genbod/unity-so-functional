using System;
using System.Collections.Generic;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public interface IStateMachine
    {
        IReadOnlyList<string> CurrentState { get; }
        event Action<string> StateChanged;
        string ToDOT(List<string> output = null);
    }
}