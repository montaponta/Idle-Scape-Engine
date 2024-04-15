using UnityEngine;

public class ExtensionClasses
{
	public static Vector3Serialized ConvertToVector3Serialized(Vector3 vector3)
	{
		return new Vector3Serialized(vector3.x, vector3.y, vector3.z);
	}

	public static Vector3 ConvertToVector3(Vector3Serialized vector3Serialized)
	{
		return new Vector3(vector3Serialized.x, vector3Serialized.y, vector3Serialized.z);
	}

	public static bool IsVector3SerializedEqualToZero(Vector3Serialized vector3)
	{
		return vector3.x == 0 && vector3.y == 0 && vector3.z == 0;
	}
}

[System.Serializable]
public struct Vector3Serialized
{
	public float x, y, z;

	public Vector3Serialized(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}
