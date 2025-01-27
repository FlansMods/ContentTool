using System.Text.RegularExpressions;
using UnityEngine;

public static class Minecraft
{
	public static readonly Vector3 Right = new Vector3(1f, 0f, 0f);
	public static readonly Vector3 Up = new Vector3(0f, 1f, 0f);
	public static readonly Vector3 Forward = new Vector3(0f, 0f, -1f);

	public static readonly Vector3 Left = new Vector3(-1f, 0f, 0f);
	public static readonly Vector3 Down = new Vector3(0f, -1f, 0f);
	public static readonly Vector3 Back = new Vector3(0f, 0f, 1f);

	public static Vector3 XYZ(float x, float y, float z) { return new Vector3(x, y, -z); }

	public static Quaternion Euler(Vector3 v)
	{
		return Quaternion.Euler(-v.x, v.y, v.z);
	}
	public static Quaternion Euler(float pitch, float yaw, float roll)
	{
		return Quaternion.Euler(-pitch, yaw, roll);
	}

	public static string SanitiseID(string shortName)
	{
		if (shortName == null || shortName.Length == 0)
			return "";
		return Regex.Replace(shortName, "([a-z])([A-Z])", "$1_$2").ToLower();
	}

	// To import a part name, such as "gunModel" from a <= 1.12 version
	public static string ConvertPartName(string partName)
	{
		if (partName.EndsWith("Model"))
			partName = partName.Substring(0, partName.Length - 5);
		else if (partName.EndsWith("Models"))
			partName = partName.Substring(0, partName.Length - 6);

		switch (partName)
		{
			// The root of a model is always called "body" now
			case "gun":
			case "plane":
			case "core":
				return "body";

			case "pump": return "pump";
			case "ammo": return "ammo_0";
			case "defaultGrip": return "grip";
			case "defaultBarrel": return "barrel";
			case "defaultScope": return "sights";
			case "defaultStock": return "stock";
			case "revolverBarrel": return "revolver";
			case "breakAction": return "break_action";
			default: return SanitiseID(partName);
		}
	}
}