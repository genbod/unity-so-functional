using UnityEngine;

[CreateAssetMenu]
public class FloatVariable : ScriptableValue<float>
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
    public float GetValue()
        => Value.Match(
            () => 0,
            (f) => f);
}
