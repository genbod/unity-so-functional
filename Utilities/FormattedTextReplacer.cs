﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using static F;

public class FormattedTextReplacer : SerializedMonoBehaviour 
{
	public Func<System.Object, Option<string>> GetFormattedValueToString;

	public Func<Option<System.Object>> GetValue;

	public Text Text;

	public bool AlwaysUpdate;

	private void OnEnable()
	{
		Text.text = GetValue().Bind(GetFormattedValueToString)
			.Match(
				() => "None",
				(f) => f
			);
	}
	
	// Update is called once per frame
	void Update () {
		if (AlwaysUpdate)
		{
			Text.text = GetValue().Bind(GetFormattedValueToString)
			.Match(
				() => "None",
				(f) => f
			);
		}
	}
}
