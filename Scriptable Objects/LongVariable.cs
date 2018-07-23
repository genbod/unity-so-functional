using System;
using UnityEngine;

[CreateAssetMenu]
public class LongVariable : ScriptableValue<long>, IPrintableValue
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
