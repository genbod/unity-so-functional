using DragonDogStudios.UnitySoFunctional.Core;
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