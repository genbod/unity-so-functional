using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DateTimeDrawer : OdinValueDrawer<DateTime>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var value = this.ValueEntry.SmartValue;

        var YYYYMMDDhhmmssRect = EditorGUILayout.GetControlRect();
        YYYYMMDDhhmmssRect = EditorGUI.IndentedRect(YYYYMMDDhhmmssRect);

        if (label != null)
        {
            YYYYMMDDhhmmssRect = EditorGUI.PrefixLabel(YYYYMMDDhhmmssRect, label);
        }

        var currYear = value.Year;
        var currMonth = value.Month;
        var currDay = value.Day;
        var currHour = value.Hour;
        var currMin = value.Minute;
        var currSec = value.Second;

        var newYear = SirenixEditorFields.IntField(YYYYMMDDhhmmssRect.Split(0, 6), currYear);
        var newMonth = SirenixEditorFields.IntField(YYYYMMDDhhmmssRect.Split(1, 6), currMonth);
        var newDay = SirenixEditorFields.IntField(YYYYMMDDhhmmssRect.Split(2, 6), currDay);
        var newHour = SirenixEditorFields.IntField(YYYYMMDDhhmmssRect.Split(3, 6), currHour);
        var newMin = SirenixEditorFields.IntField(YYYYMMDDhhmmssRect.Split(4, 6), currMin);
        var newSec = SirenixEditorFields.IntField(YYYYMMDDhhmmssRect.Split(5, 6), currSec);

        if (currYear != newYear || currMonth != newMonth || currDay != newDay ||
            currHour != newHour || newMin != currMin || newSec != currSec)
        {
            this.ValueEntry.SmartValue = new DateTime(
                year: newYear,
                month: newMonth,
                day: newDay,
                hour: newHour,
                minute: newMin,
                second: newSec);
            this.ValueEntry.ApplyChanges();
        }
    }
}
