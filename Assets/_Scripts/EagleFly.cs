using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EagleFly : MonoBehaviour {
    public Animator animator;
	// Use this for initialization
	void Start ()
    {
        animator.SetBool("Fly", true);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            animator.SetBool("Fly", true);
        }
	}
}
