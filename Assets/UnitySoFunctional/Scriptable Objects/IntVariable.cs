using DragonDogStudios.UnitySoFunctional.Core;
using DragonDogStudios.UnitySoFunctional.Functional;
using DragonDogStudios.UnitySoFunctional.Utilities;
using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.ScriptableObjects
{
    [CreateAssetMenu]
    public class IntVariable : ScriptableValue<int>, IPrintableValue
    {
        public string GetValueToString(bool prettyPrint)
        {
            if (prettyPrint)
            {
                return Value.Map(StringHelper.GetPrettyNumber)
                    .Match(
                    () => "None",
                    (f) => f);
            }
            return Value.Match(
                () => "None",
                (f) => f.ToString());
        }

        public string GetFormattedValueToString()
        {
            return Value.Match(
                () => "None",
                (f) => StringHelper.GetFormattedInt(f)
            );
        }

        public int GetValue()
            => Value.Match(
                () => 0,
                (f) => f);
    }
}