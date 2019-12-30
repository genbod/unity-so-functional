using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    [CreateAssetMenu]
    public class BoolVariable : ScriptableValue<bool>, IReturnValue<bool>
    {
        public bool GetValue()
        {
            return Value.Match(
                () => false,
                (f) => f);
        }
    }
}