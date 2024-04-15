using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSelector : PropertyAttribute
{
    public Type targetType;

	public CustomSelector(Type type)
	{
		targetType = type;
	}
}
