using DragonDogStudios.UnitySoFunctional.Core;
using DragonDogStudios.UnitySoFunctional.Events;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    [CreateAssetMenu]
    public class StringVariable : ScriptableValue<string>, IPrintableValue
    {
        public string GetValueToString(bool prettyPrint = false)
        {
            return Value.Match(
                () => "None",
                (f) => f);
        }
    }
}