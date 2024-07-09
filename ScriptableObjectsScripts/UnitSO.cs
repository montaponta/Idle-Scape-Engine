using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "ScriptableObjects/UnitData")]
public class UnitSO : ScriptableObject, IUnitData
{
	public float moveSpeed;
	public List<AdditionalParameters> additionalParameters;

	public object GetValueByTag(string tag, System.Type returnType)
	{
		var v = additionalParameters.Find(a => a.paramName == tag);

		if (v != null)
		{
			if (returnType == typeof(float))
			{
				System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");
				return float.Parse(v.value, cultureInfo);
			}

			if (returnType == typeof(string)) return v.value;
		}

		return null;
	}

    public T GetValueByTag<T>(string tag)
    {
        var v = (T)GetValueByTag(tag, typeof(T));
        return v != null ? v : default(T);
    }

    public float GetMoveSpeed()
	{
		return moveSpeed;
	}

	public int GetHealth()
	{
		return 100;
	}
}

