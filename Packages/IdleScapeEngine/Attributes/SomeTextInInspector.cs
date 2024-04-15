using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeTextInInspector : PropertyAttribute
{
	public readonly string text;
	public readonly FontStyle fontStyle;

	public SomeTextInInspector(string text, FontStyle fontStyle)
	{
		this.text = text;
		this.fontStyle = fontStyle;
	}
}
