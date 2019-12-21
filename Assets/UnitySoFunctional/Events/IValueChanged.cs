using System.ComponentModel;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    interface IValueChanged<T>
    {
        ValueChangedEvent<T> ValueChangedEvent { get; }
        void FireEvent();
    }
}