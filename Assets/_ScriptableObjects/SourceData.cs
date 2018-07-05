using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class SourceData : ScriptableObject
{
	[InlineEditor]
	[MakeLocalMenu]
	public StringVariable Title;

	[InlineEditor]
	[MakeLocalMenu]
	public IntVariable Count;

	[InlineEditor]
	[MakeLocalMenu]
	public BoolVariable Enabled;

	[InlineEditor]
	[MakeLocalMenu]
	public IntVariable Index;

	private void Awake()
	{
		if (Title == null)
		{
			Title = ScriptableObject.CreateInstance<StringVariable>();
		}
		if (Count == null)
		{
			Count = ScriptableObject.CreateInstance<IntVariable>();
		}
		if (Enabled == null)
		{
			Enabled = ScriptableObject.CreateInstance<BoolVariable>();
		}
		if (Index == null)
		{
			Index = ScriptableObject.CreateInstance<IntVariable>();
		}
	}
}
