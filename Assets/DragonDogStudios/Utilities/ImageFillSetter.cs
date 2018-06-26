using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFillSetter : MonoBehaviour
{

    [Tooltip("Value to use as the current ")]

    public FloatVariable Variable;



    [Tooltip("Min value that Variable to have no fill on Image.")]

    public FloatVariable Min;



    [Tooltip("Max value that Variable can be to fill Image.")]

    public FloatVariable Max;



    [Tooltip("Image to set the fill amount on.")]

    public Image Image;



    private void Update()
    {
        if (Image == null || Variable == null || Min == null || Max == null)
        {
            return;
        }

        Image.fillAmount = Mathf.Clamp01(
            Mathf.InverseLerp(Min.GetValue(), Max.GetValue(), Variable.GetValue()));
    }
}
