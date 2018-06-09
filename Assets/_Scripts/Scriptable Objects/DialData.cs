using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class DialData : ScriptableObject {

    [InlineEditor]
    public StringVariable Name;

    [InlineEditor]
    public IntVariable Count;

    public void OnEnable()
    {
        if (Name == null)
        {
            Name = new StringVariable();
        }
        if (Count == null)
        {
            Count = new IntVariable();
        }
    }
}
