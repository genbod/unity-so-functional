using UnityEngine;

[CreateAssetMenu]
public class BoolVariable : ScriptableValue<bool>
{
    public bool GetValue()
    {
        return Value.Match(
            () => false,
            (f) => f);
    }
}
