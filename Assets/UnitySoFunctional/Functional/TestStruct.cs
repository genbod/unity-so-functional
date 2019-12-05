using Sirenix.Serialization;
using System;

namespace DragonDogStudios.UnitySoFunctional.Functional
{
    [Serializable]
    public class TestStruct<T>
    {
        [OdinSerialize]
        private T value;
        [OdinSerialize]
        private bool isSome;
        bool isNone => !isSome;

        private TestStruct(T value)
        {
            if (value == null)
                throw new ArgumentNullException();
            this.isSome = true;
            this.value = value;
        }
    }
}