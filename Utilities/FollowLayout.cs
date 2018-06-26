using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLayout : MonoBehaviour {
    public List<Transform> layoutTargets;

    public Transform deepSix;

    public IntVariable index;

    public float smoothTime = .3F;

    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void Update() {
        if (index == null || index.Value == null || layoutTargets == null || transform == null)
        {
            return;
        }

        Vector3 targetPosition = index.Value.Match(
            () => transform.position,
            (f) => (f < layoutTargets.Count) && (f > -1) ? layoutTargets[f].position : deepSix.position);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
	}
}
