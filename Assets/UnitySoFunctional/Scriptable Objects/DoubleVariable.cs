using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
