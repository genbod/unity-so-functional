using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField]
    Material material;

    public void ChangeColor(string htmlColor)
    {
        if (material != null)
        {
            Color newColor;
            ColorUtility.TryParseHtmlString(htmlColor, out newColor);
            material.SetColor("_Color", newColor);
        }
    }
}
