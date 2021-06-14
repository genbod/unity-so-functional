using UnityEngine.Events;

namespace DragonDogStudios.UnitySoFunctional.StateMachines
{
    public interface IState
    {
        string Name { get; }
        void Tick();
        void OnEnter();
        void OnExit();
        void SetTickActions(UnityEvent onTick);
        void SetOnEnterActions(UnityEvent onEnter);
        void SetOnExitActions(UnityEvent onExit);
    }
}