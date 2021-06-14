using System;
using System.Collections.Generic;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public interface IStateMachine: IDisposable
    {
        IReadOnlyList<string> CurrentState { get; }
        event Action<string> StateChanged;
        string ToDOT(List<string> output = null);
        void Tick();
        void Exit();
        void Enter();
    }
}