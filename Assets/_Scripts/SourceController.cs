using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SourceController : SerializedMonoBehaviour 
{
	public SourceData sourceData;
	public List<Transform> SourceLocations;
	public Transform DeepSix;
	public StringGameEvent SourceClickedEvent;

	// Local to prefab
	public TextReplacer Title;
	public FormattedTextReplacer Count;
	public FollowLayout FollowLayout;
	public EnabledSetter Enabler;

	private void Start()
	{
		Title.GetValueToString = sourceData.Title.GetValueToString;
		Count.GetValue = sourceData.Count.GetValueAsOption;
		FollowLayout.index = sourceData.Index;
		FollowLayout.layoutTargets = SourceLocations;
		FollowLayout.deepSix = DeepSix;
		Enabler.Enabled = sourceData.Enabled;
	}

	public void SendClickMessage()
	{
		var message = sourceData.Title.Value.Map((f)=> f)
			.Match(()=> "Missing Source Name",
			(f) => f);
		SourceClickedEvent.Raise(message);
	}
}
