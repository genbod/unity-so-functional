using UnityEngine;

[CreateAssetMenu]
public class StringVariable : ScriptableValue
{
    public string Value;

    public override string GetValueToString(bool prettyPrint)
    {
        return Value;
    }
}