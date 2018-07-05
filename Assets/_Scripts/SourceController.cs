using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceController : MonoBehaviour 
{
	public SourceData sourceData;
	public List<Transform> SourceLocations;
	public Transform DeepSix;

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
}
