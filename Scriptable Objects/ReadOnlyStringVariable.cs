using UnityEngine;
using static F;

[CreateAssetMenu]
public class ReadOnlyStringVariable : StringVariable
{
    public ReadOnlyStringVariable(string value)
    {
        base.SetValue(value);
    }

    private new void SetValue(Option<string> newValue)
        => base.SetValue(newValue);

    private new void SetValue(string newValue)
        => base.SetValue(newValue);
}