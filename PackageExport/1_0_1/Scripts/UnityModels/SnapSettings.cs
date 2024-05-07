using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PositionSnapSetting
{
	Free,
	OneOver16,
	OneOver8,
	OneOver4,
	OneOver2,
	One,
	Two,
	Four,
	Eight,
	Sixteen
}

public enum RotationSnapSetting
{
	Free,
	Deg1,
	Deg2,
	Deg5,
	Deg10,
	Deg15,
	Deg22_5,
	Deg30,
	Deg45,
	Deg60,
	Deg90,
}

public static class SnapSettings
{
	public static readonly string[] PosSnapNames = new string[] {
		"Free", "1/16", "1/8", "1/4", "1/2", "1", "2", "4", "8", "16"
	};
	public static readonly string[] RotSnapNames = new string[] {
		"Free", "1°", "2°", "5°", "10°", "15°", "22.5°", "30°", "45°", "60°", "90°"
	};

	public static float Increment(this RotationSnapSetting rotSnap)
	{
		switch(rotSnap)
		{
			case RotationSnapSetting.Free:		return 0.0f;
			case RotationSnapSetting.Deg1:		return 1.0f;
			case RotationSnapSetting.Deg2:		return 2.0f;
			case RotationSnapSetting.Deg5:		return 5.0f;
			case RotationSnapSetting.Deg10:		return 10.0f;
			case RotationSnapSetting.Deg15:		return 15.0f;
			case RotationSnapSetting.Deg22_5:	return 22.5f;
			case RotationSnapSetting.Deg30:		return 30.0f;
			case RotationSnapSetting.Deg45:		return 45.0f;
			case RotationSnapSetting.Deg60:		return 60.0f;
			case RotationSnapSetting.Deg90:		return 90.0f;
			default:							return 0.0f;
		}
	}
	public static float SnapRadians(this RotationSnapSetting rotSnap, float input)
	{
		return SnapDegrees(rotSnap, input * Mathf.Rad2Deg) * Mathf.Deg2Rad;
	}
	public static Vector3 SnapEulers(this RotationSnapSetting rotSnap, Vector3 input)
	{
		return new Vector3(SnapDegrees(rotSnap, input.x), SnapDegrees(rotSnap, input.y), SnapDegrees(rotSnap, input.z));
	}
	public static float SnapDegrees(this RotationSnapSetting rotSnap, float input)
	{
		switch (rotSnap)
		{
			case RotationSnapSetting.Free:			return input;
			case RotationSnapSetting.Deg1:			return Mathf.Round(input);
			case RotationSnapSetting.Deg2:			return Mathf.Round(input / 2f) * 2f;
			case RotationSnapSetting.Deg5:			return Mathf.Round(input / 5f) * 5f;
			case RotationSnapSetting.Deg10:			return Mathf.Round(input / 10f) * 10f;
			case RotationSnapSetting.Deg15:			return Mathf.Round(input / 15f) * 15f;
			case RotationSnapSetting.Deg22_5:		return Mathf.Round(input / 22.5f) * 22.5f;
			case RotationSnapSetting.Deg30:			return Mathf.Round(input / 30f) * 30f;
			case RotationSnapSetting.Deg45:			return Mathf.Round(input / 45f) * 45f;
			case RotationSnapSetting.Deg60:			return Mathf.Round(input / 60f) * 60f;
			case RotationSnapSetting.Deg90:			return Mathf.Round(input / 90f) * 90f;
			default:								return input;
		}
	}

	public static float Increment(this PositionSnapSetting posSnap)
	{
		switch (posSnap)
		{
			case PositionSnapSetting.Free:		return 0.0f;
			case PositionSnapSetting.OneOver16: return 1f / 16f;
			case PositionSnapSetting.OneOver8:	return 1f / 8f;
			case PositionSnapSetting.OneOver4:	return 1f / 4f;
			case PositionSnapSetting.OneOver2:	return 1f / 2f;
			case PositionSnapSetting.One:		return 1f;
			case PositionSnapSetting.Two:		return 2f;
			case PositionSnapSetting.Four:		return 4f;
			case PositionSnapSetting.Eight:		return 8f;
			case PositionSnapSetting.Sixteen:	return 16f;
			default: return 0.0f;
		}
	}
	public static Vector3 Snap(this PositionSnapSetting posSnap, Vector3 input)
	{
		return new Vector3(Snap(posSnap, input.x), Snap(posSnap, input.y), Snap(posSnap, input.z));
	}
	public static float Snap(this PositionSnapSetting posSnap, float input)
	{
		switch (posSnap)
		{
			case PositionSnapSetting.Free:			return input;
			case PositionSnapSetting.OneOver16:		return Mathf.Round(input * 16f) / 16f;
			case PositionSnapSetting.OneOver8:		return Mathf.Round(input * 8f) / 8f;
			case PositionSnapSetting.OneOver4:		return Mathf.Round(input * 4f) / 4f;
			case PositionSnapSetting.OneOver2:		return Mathf.Round(input * 2f) / 2f;
			case PositionSnapSetting.One:			return Mathf.Round(input);
			case PositionSnapSetting.Two:			return Mathf.Round(input / 2f) * 2f;
			case PositionSnapSetting.Four:			return Mathf.Round(input / 4f) * 4f;
			case PositionSnapSetting.Eight:			return Mathf.Round(input / 8f) * 8f;
			case PositionSnapSetting.Sixteen:		return Mathf.Round(input / 16f) * 16f;
			default:								return input;
		}
	}
}
