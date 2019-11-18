using DragonDogStudios.UnitySoFunctional.Events;
using DragonDogStudios.UnitySoFunctional.Exceptions;
using DragonDogStudios.UnitySoFunctional.Functional;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using static DragonDogStudios.UnitySoFunctional.Functional.F;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    public class ScriptableValue<T> : SerializedScriptableObject, IValueChanged<T>
    {
        [OdinSerialize]
        protected bool _lock = false;
        public Option<T> DefaultValue;

        [OdinSerialize]
        private Option<T> _value;
        public Option<T> Value
        {
            set
            {
                if (_lock)
                {
                    throw new ReadOnlyObjectEditException();
                }
                _value = value;
            }
            get
            {
                return _value;
            }
        }

        private ValueChangedEvent<T> _valueChangedEvent;

        public ValueChangedEvent<T> ValueChangedEvent
        {
            get
            {
                if (_valueChangedEvent == null)
                {
                    _valueChangedEvent = new ValueChangedEvent<T>();
                }
                return _valueChangedEvent;
            }
        }

        public void SetValue(Option<T> newValue)
            => Value = newValue;

        public void SetValue(T newValue)
        {
            Value = Some(newValue);
            ValueChangedEvent.Raise(newValue);
        }

        private void OnEnable()
        {
            if (DefaultValue.IsSome())
            {
                Value = DefaultValue;
            }
        }

        public Option<System.Object> GetValueAsOption()
        {
            // This doesn't work for None becasue returned value for None is converted to Option<Option.None>
            return Value.Match(
                () => None,
                (f) => Some((System.Object)f)
            );
        }

        public static V CreateAsReadOnly<V>(V instance) where V : ScriptableValue<T>
        {
            instance._lock = true;
            return instance;
        }

        private void OnValidate()
        {
            _value.ForEach(ValueChangedEvent.Raise);
        }
    }
}