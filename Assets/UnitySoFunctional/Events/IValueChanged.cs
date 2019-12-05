namespace DragonDogStudios.UnitySoFunctional.Events
{
    interface IValueChanged<T>
    {
        ValueChangedEvent<T> ValueChangedEvent { get; }
    }
}