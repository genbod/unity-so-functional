using UnityEngine;

[CreateAssetMenu]
public class EnumVariable : ScriptableValue<VoiceStatus>
{
    public string GetValueToString()
    {
        return Value.Match(
            () => "None",
            (f) => f.ToString());
    }
}