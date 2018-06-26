using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialController : MonoBehaviour {
    public DialData dialData;
    public List<Transform> DialLocations;
    public Transform DeepSix;

    // Local to Prefab
    public TextReplacer Title;
    public AnimatedCounter Count;
    public ImageFillSetter Fill;
    public FollowLayout FollowLayout;
    public EnabledSetter Enabler;

    private void Start()
    {
        Title.GetValueToString = dialData.Name.GetValueToString;
        Count.GetTargetCount = dialData.Count.GetValue;
        Fill.Variable = dialData.FillAmount;
        FollowLayout.index = dialData.Index;
        FollowLayout.layoutTargets = DialLocations;
        FollowLayout.deepSix = DeepSix;
        Enabler.Enabled = dialData.Enabled;
    }
}
