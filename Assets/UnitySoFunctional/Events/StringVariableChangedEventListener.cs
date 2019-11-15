using DragonDogStudios.UnitySoFunctional.ScriptableObjects;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.Events
{
    public class StringVariableChangedEventListener : ValueChangedEventListener<string>
    {
        [SerializeField]
        private StringVariable _stringVariable;

        private void Awake()
        {
            this.Event = _stringVariable.ValueChangedEvent;
        }
    }
}