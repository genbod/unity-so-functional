using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class DialData : ScriptableObject {

    [InlineEditor]
    public IntVariable Count;

    public void OnEnable()
    {
        if (Count == null)
        {
            Count = new IntVariable();
        }
    }
}
