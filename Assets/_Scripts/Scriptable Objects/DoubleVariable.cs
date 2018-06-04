using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DoubleVariable : ScriptableValue {
    public double Value;

    public override string GetValueToString(bool prettyPrint)
    {
        if (prettyPrint)
        {
            return TextUtils.GetPrettyNumber(Value);
        }
        return Value.ToString();
    }
}
