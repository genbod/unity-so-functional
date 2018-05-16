using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedFill : MonoBehaviour {
    [SerializeField]
    public float decayRate = 1.0f;

    public Image target;

    [SerializeField]
    public float FillAmount { set { target.fillAmount = value; } }
	
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var currentFill = target.fillAmount;
        currentFill -= decayRate * Time.unscaledDeltaTime;
        currentFill = currentFill < 0 ? 0 : currentFill;
        target.fillAmount = currentFill;
	}
}
