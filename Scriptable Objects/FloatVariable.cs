using UnityEngine;

[CreateAssetMenu]
public class FloatVariable : ScriptableValue<float>
{
    public float GetValue()
        => Value.Match(
            () => 0,
            (f) => f);
}
