using UnityEngine;

public static class Minecraft
{
	public static readonly Vector3 Right = new Vector3(1f, 0f, 0f);
	public static readonly Vector3 Up = new Vector3(0f, 1f, 0f);
	public static readonly Vector3 Forward = new Vector3(0f, 0f, -1f);

	public static readonly Vector3 Left = new Vector3(-1f, 0f, 0f);
	public static readonly Vector3 Down = new Vector3(0f, -1f, 0f);
	public static readonly Vector3 Back = new Vector3(0f, 0f, 1f);
	public static Quaternion Euler(Vector3 v)
	{
		return Quaternion.Euler(-v.x, v.y, v.z);
	}
	public static Quaternion Euler(float pitch, float yaw, float roll)
	{
		return Quaternion.Euler(-pitch, yaw, roll);
	}
}