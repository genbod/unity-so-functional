using DragonDogStudios.UnitySoFunctional.Events;
using DragonDogStudios.UnitySoFunctional.Exceptions;
using DragonDogStudios.UnitySoFunctional.Functional;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using static DragonDogStudios.UnitySoFunctional.Functional.F;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    public class ScriptableValue<T> : SerializedScriptableObject, IValueChanged<T>, IScriptableValue
    {
        [OdinSerialize]
        protected bool _lock = false;
        public Option<T> DefaultValue;

        [OdinSerialize]
        private Option<T> _value;
        public Option<T> Value
        {
            private set
            {
                if (_lock)
                {
                    throw new ReadOnlyObjectEditException();
                }
                _value = value;
                _value.ForEach(ValueChangedEvent.Raise);
            }
            get => _value;
        }

        private ValueChangedEvent<T> _valueChangedEvent;

        public ValueChangedEvent<T> ValueChangedEvent => _valueChangedEvent ?? (_valueChangedEvent = new ValueChangedEvent<T>());

        private void OnEnable()
        {
            if (DefaultValue.IsSome())
            {
                Value = DefaultValue;
            }
        }

        [Button]
        public void Reset()
        {
            Value = DefaultValue;
        }

        [Button]
        public void FireEvent()
        {
            Value.ForEach(ValueChangedEvent.Raise);
        }

        public void SetValue(Option<T> newValue)
            => Value = newValue;

        public void SetValue(T newValue)
        {
            Value = Some(newValue);
        }

        public Option<System.Object> GetValueAsOption()
        {
            // This doesn't work for None becasue returned value for None is converted to Option<Option.None>
            return Value.Match(
                () => None,
                (f) => Some((System.Object)f)
            );
        }
        
        // This is needed to trigger updates as soon as value is changed in editor.
        private void OnValidate()
        {
            Value.ForEach(ValueChangedEvent.Raise);
        }

        public static V CreateAsReadOnly<V>(V instance) where V : ScriptableValue<T>
        {
            instance._lock = true;
            return instance;
        }
    }
}