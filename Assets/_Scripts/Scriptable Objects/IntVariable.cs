using UnityEngine;

[CreateAssetMenu]
public class IntVariable : ScriptableValue
{
    public int Value;
    

    public override string GetValueToString(bool prettyPrint)
    {
        if (prettyPrint)
        {
            return TextUtils.GetPrettyNumber(Value);
        }
        return Value.ToString();
    }
}
