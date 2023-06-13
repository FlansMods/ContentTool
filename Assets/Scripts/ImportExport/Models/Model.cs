using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EFace
{
	north, east, south, west, down, up
}

[System.Serializable]
public class Model
{
	public enum ModelType
	{
		None,
		TurboRig,
		Block,
		Item,
	}

	public ModelType Type = ModelType.None;

	[Header("Minecraft Block")]
	public string top;
	public string north;
	public string east;
	public string south;
	public string west;
	public string bottom;

	[Header("Minecraft Item")]
	public string icon;

	[Header("TurboRig")]
	public int textureX;
	public int textureY;
	public List<Section> sections = new List<Section>();
	public string name;
	public List<AnimationParameter> animations = new List<AnimationParameter>();
	public List<AttachPoint> attachPoints = new List<AttachPoint>();
	
	public enum EShape
	{
		Box,
		ShapeBox,
	}

	[System.Serializable]
	public class Piece
	{
        public int textureU, textureV;

        public Vector3 Pos = Vector3.zero;
		public Vector3 Dim = Vector3.zero;
		public Vector3 Origin = Vector3.zero;
		public Vector3 Euler = Vector3.zero;
		public EShape Shape = EShape.Box;

		// For shapeboxes
		public Vector3[] Offsets = new Vector3[8];

		public void DoMirror(bool bX, bool bY, bool bZ)
        {
            // TODO: Fill in
        }

		public int[] GetIntUV(int u0, int v0, EFace face)
		{
			int x = Mathf.CeilToInt(Dim.x), y = Mathf.CeilToInt(Dim.y), z = Mathf.CeilToInt(Dim.z);
			switch(face)
			{
				case EFace.west: return new int[] {  u0, 			v0 + z, 	u0 + z, 		v0 + z+y };
				case EFace.south: return new int[] { u0 + z, 		v0 + z, 	u0 + z+x, 		v0 + z+y };
				case EFace.east: return new int[] {  u0 + z+x, 		v0 + z, 	u0 + z+x+z, 	v0 + z+y };
				case EFace.north: return new int[] { u0 + z+x+z, 	v0 + z, 	u0 + z+x+z+x, 	v0 + z+y };
				case EFace.down: return new int[] {  u0 + z, 		v0, 		u0 + z+x, 		v0 + z   };
				case EFace.up: return new int[] { 	 u0 + z+x, 		v0, 		u0 + z+x+x, 	v0 + z   };
				default: return new int[4];
			}
		}		

		public Vector2[] GetUVS(EFace face, int tu, int tv)
		{
			float x = Mathf.Ceil(Dim.x);
            float y = Mathf.Ceil(Dim.y);
            float z = Mathf.Ceil(Dim.z);

			switch(face)
			{
				case EFace.north: return new Vector2[] {
				    new Vector2(tu + x * 2 + z * 2, tv + y + z),
					new Vector2(tu + x * 2 + z * 2, tv + z),
					new Vector2(tu + x + z * 2, tv + z),
					new Vector2(tu + x + z * 2, tv + y + z),
				};
				case EFace.south: return new Vector2[] {
				    new Vector2(tu + x + z, tv + y + z),
					new Vector2(tu + x + z, tv + z),
					new Vector2(tu + z, tv + z),
            		new Vector2(tu + z, tv + y + z),
				};
				case EFace.west: return new Vector2[] {
					new Vector2(tu + z, tv + y + z),
					new Vector2(tu + z, tv + z),
					new Vector2(tu, tv + z),
					new Vector2(tu, tv + y + z)
				};
				case EFace.east: return new Vector2[] {
				    new Vector2(tu + x + 2 * z, tv + y + z),
					new Vector2(tu + x + 2 * z, tv + z),
					new Vector2(tu + x + z, tv + z),
					new Vector2(tu + x + z, tv + y + z)
				};
				case EFace.up: return new Vector2[] {
				    new Vector2(tu + x + z, tv + z),
            		new Vector2(tu + x + z, tv),
            		new Vector2(tu + z, tv),
            		new Vector2(tu + z, tv + z)
				};
				case EFace.down: return new Vector2[] {
				    new Vector2(tu + x * 2 + z, tv + z),
					new Vector2(tu + x + z, tv + z),
					new Vector2(tu + x + z, tv),
					new Vector2(tu + x * 2 + z, tv)
				};
				default:
				return new Vector2[4];
			}
		}

		public Vector3[] GetVerts()
		{
			switch(Shape)
			{
				case EShape.Box:
				{
					Quaternion xRotation = Quaternion.Euler(Euler);
					Vector3[] verts = new Vector3[8];
					for (int x = 0; x < 2; x++)
					{
						for (int y = 0; y < 2; y++)
						{
							for (int z = 0; z < 2; z++)
							{
								int index = x + y * 2 + z * 4;
								verts[index] = Pos;
								if (x == 1) verts[index].x += Dim.x;
								if (y == 1) verts[index].y += Dim.y;
								if (z == 1) verts[index].z += Dim.z;

								//verts[index] = xRotation * verts[index];
								//verts[index] += xOrigin;
							}
						}
					}

            		return verts;
				}
				case EShape.ShapeBox:
				{
					Quaternion xRotation = Quaternion.Euler(Euler);
					Vector3[] verts = new Vector3[8];
					for (int x = 0; x < 2; x++)
					{
						for (int y = 0; y < 2; y++)
						{
							for (int z = 0; z < 2; z++)
							{
								int index = x + y * 2 + z * 4;
								verts[index] = Pos;
								if (x == 1) verts[index].x += Dim.x;
								if (y == 1) verts[index].y += Dim.y;
								if (z == 1) verts[index].z += Dim.z;

								verts[index] += Offsets[index];

								//verts[index] = xRotation * verts[index];
								//verts[index] += xOrigin;
							}
						}
					}

					return verts;
				}
				default:
					return new Vector3[8];
			}
		}
	}

	[System.Serializable]
	public class Section
	{
		public string partName;
		public Piece[] pieces;
	}

	public Section GetSection(string key)
	{
		foreach(Section section in sections)
			if(section.partName == key)
				return section;
		return null;
	}

	public bool TryGetFloatParam(string key, out float value)
	{
		foreach(AnimationParameter parameter in animations)
			if(parameter.key == key)
			{
				value = parameter.floatValue;
				return true;
			}
		value = 0.0f;
		return false;
	}

	public bool TryGetVec3Param(string key, out Vector3 value)
	{
		foreach(AnimationParameter parameter in animations)
			if(parameter.key == key)
			{
				value = parameter.vec3Value;
				return true;
			}
		value = Vector3.zero;
		return false;
	}

	[System.Serializable]
	public class AnimationParameter 
	{
		public string key = "";
		public bool isVec3 = false;
		public float floatValue = 0.0f;
		public Vector3 vec3Value = Vector3.zero;
	}

	[System.Serializable]
	public class AttachPoint 
	{
		public string name = "";
		public string attachedTo = "";
		public Vector3 position = Vector3.zero;
	}
	public AttachPoint GetOrCreate(string name)
	{
		foreach(AttachPoint point in attachPoints)
			if(point.name == name)
				return point;
		AttachPoint point1 = new AttachPoint()
		{
			name = name,
			attachedTo = "body",
			position = Vector3.zero,
		};
		attachPoints.Add(point1);
		return point1;
	}
	public void SetAttachment(string name, string attachedTo)
	{
		AttachPoint ap = GetOrCreate(name);
		ap.attachedTo = attachedTo;
	}
	public void SetAttachmentOffset(string name, Vector3 offset)
	{
		AttachPoint ap = GetOrCreate(name);
		ap.position = offset;
	}
}
