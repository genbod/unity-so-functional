using DragonDogStudios.UnitySoFunctional.Core;
using DragonDogStudios.UnitySoFunctional.Functional;
using DragonDogStudios.UnitySoFunctional.Utilities;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    [CreateAssetMenu]
    public class DoubleVariable : ScriptableValue<double>, IPrintableValue
    {
        public string GetValueToString(bool prettyPrint)
        {
            if (prettyPrint)
            {
                return Value.Map(StringHelper.GetPrettyNumber)
                    .Match(
                    () => "",
                    (f) => f);
            }
            return Value.ToString();
        }
    }
}