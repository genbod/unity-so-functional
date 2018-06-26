using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedCounter : SerializedMonoBehaviour
{

    [SerializeField]
    public bool prettyPrint = false;

    [SerializeField]
    public float changeRate = 1.0f;
    
    public Func<int> GetTargetCount;

    public Text Text;

    private float multiplier = 1;

    private int currentCount;

    private void OnEnable()
    {
        currentCount = GetTargetCount();
        SetText();
    }

    // Update is called once per frame
    void Update()
    {
        var targetCount = GetTargetCount();

        // Scale multiplier based on how much we have to count
        var zeros = Mathf.Max(Mathf.Abs(currentCount - targetCount).ToString().Length, 2);
        multiplier = Mathf.Pow(10, zeros);
        var change = Mathf.CeilToInt(Time.unscaledDeltaTime * changeRate * multiplier);
        if (currentCount < targetCount)
        {
            currentCount += change;
            if (currentCount > targetCount)
            {
                currentCount = targetCount;
            }
        }
        else if (currentCount > targetCount)
        {
            currentCount -= change;
            if (currentCount < targetCount)
            {
                currentCount = targetCount;
            }
        }

        SetText();
    }

    private void SetText()
    {
        if (prettyPrint)
        {
            Text.text = StringHelper.GetPrettyNumber(currentCount);
        }
        else
        {
            Text.text = StringHelper.GetFormattedInt(currentCount);
        }
    }
}

