using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedCounter : MonoBehaviour {

    [SerializeField]
    public bool prettyPrint = false;

    [SerializeField]
    public float changeRate = 1.0f;

    [SerializeField]
    public int TargetCount = 0;

    public Text target;

    private float multiplier = 1;

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        int currentCount = 0;
        if (!prettyPrint)
        {
            int.TryParse(target.text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out currentCount);
        }
        else
        {
            var floatCount = float.Parse(target.text.Substring(0, target.text.Length - 1));
            var identifier = target.text.Substring(target.text.Length - 1);
            if (identifier == "M")
            {
                currentCount = (int)(1000000 * floatCount);
            }
            else if (identifier == "K")
            {
                currentCount = (int)(1000 * floatCount);
            }
        }
        // Scale multiplier based on how much we have to count
        var zeros = Mathf.Max(Mathf.Abs(currentCount - TargetCount).ToString().Length, 2);
        multiplier = Mathf.Pow(10, zeros);
        var change = Mathf.CeilToInt(Time.unscaledDeltaTime * changeRate * multiplier);
        if (currentCount < TargetCount)
        {
            currentCount += change;
            if (currentCount > TargetCount)
            {
                currentCount = TargetCount;
            }
        }
        else if (currentCount > TargetCount)
        {
            currentCount -= change;
            if (currentCount < TargetCount)
            {
                currentCount = TargetCount;
            }
        }

        if (prettyPrint)
        {
            target.text = TextUtils.GetPrettyNumber(currentCount);
        }
        else
        {
            target.text = currentCount.ToString("#,#", CultureInfo.InvariantCulture);
        }
	}

    public void SetCurrentCount(int currentCount)
    {
        if (prettyPrint)
        {
            target.text = TextUtils.GetPrettyNumber(currentCount);
            TargetCount = currentCount;
        }
        else
        {
            target.text = currentCount.ToString("#,#", CultureInfo.InvariantCulture);
            TargetCount = currentCount;
        }
    }
}
