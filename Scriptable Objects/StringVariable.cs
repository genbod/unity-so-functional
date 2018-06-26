using UnityEngine;
using static F;

[CreateAssetMenu]
public class StringVariable : ScriptableValue<string>, IPrintableValue
{
    public string GetValueToString(bool prettyPrint)
    {
        return Value.Match(
            () => "None",
            (f) => f);
    }
}