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
		Trapezoid
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

		public Piece Copy()
		{
			return new Piece() {
				textureU = this.textureU,
				textureV = this.textureV,
				Pos = this.Pos,
				Dim = this.Dim,
				Origin = this.Origin,
				Euler = this.Euler,
				Shape = this.Shape,
				Offsets = new Vector3[] {
					Offsets[0], Offsets[1], Offsets[2], Offsets[3], 
					Offsets[4], Offsets[5], Offsets[6], Offsets[7],
				},
			};
		}

		public void DoMirror(bool bX, bool bY, bool bZ)
        {
			if(bX)
			{
				Pos.x = -Pos.x - Dim.x;
				Origin.x = -Origin.x;
			}
			if(bY)
			{
				Pos.y = -Pos.y - Dim.y;
				Origin.y = -Origin.y;
			}
			if(bZ)
			{
				Pos.z = -Pos.z - Dim.z;
				Origin.z = -Origin.z;
			}
			Offsets = JavaModelImporter.MirrorOffsets(Offsets, bX, bY, bZ);
        }

		public void GetBounds(out Vector3 min, out Vector3 max)
		{
			Vector3[] verts = GetVerts();
			min = Vector3.one * 1000f;
			max = Vector3.one * -1000f;
			for(int i = 0; i < verts.Length; i++)
			{
				verts[i] = Quaternion.Euler(Euler) * verts[i];
				verts[i] += Origin;
				min = Vector3.Min(min, verts[i]);
				max = Vector3.Max(max, verts[i]);
			}
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

		public int[] GetTris()
		{
			return new int[] {
				0,2,3,  0,3,1, // -z 0, 2, 3, 1, 
				5,7,6,  5,6,4, // +z 5, 7, 6, 4,
				4,6,2,  4,2,0, // -x 4, 6, 2, 0,
				1,3,7,  1,7,5, // +x 1, 3, 7, 5,
				7,3,2,  7,2,6, // +y 7, 3, 2, 6,
				5,4,0,  5,0,1, // -y 5, 4, 0, 1 
			};
		}

		public void ExportToMesh(Mesh mesh, float textureX, float textureY)
		{
			Vector3[] v = GetVerts();
			mesh.SetVertices(new Vector3[] {
				v[0], v[2], v[3], v[1],	// -z face
				v[5], v[7], v[6], v[4], // +z face
				v[4], v[6], v[2], v[0], // -x face
				v[1], v[3], v[7], v[5], // +x face
				v[7], v[3], v[2], v[6], // +y face
				v[5], v[4], v[0], v[1], // -y face
			});
			List<Vector2> uvs = new List<Vector2>();
			uvs.AddRange(GetUVS(EFace.north, textureU, textureV));
			uvs.AddRange(GetUVS(EFace.south, textureU, textureV));
			uvs.AddRange(GetUVS(EFace.west, textureU, textureV));
			uvs.AddRange(GetUVS(EFace.east, textureU, textureV));
			uvs.AddRange(GetUVS(EFace.up, textureU, textureV));
			uvs.AddRange(GetUVS(EFace.down, textureU, textureV));
			for(int i = 0; i < uvs.Count; i++)
			{
				uvs[i] = new Vector2(uvs[i].x / textureX, 1.0f - uvs[i].y / textureY);
			}
			mesh.SetUVs(0, uvs);
			mesh.SetTriangles(new int[] {
				0,1,2, 0,2,3,
				4,5,6, 4,6,7,
				8,9,10, 8,10,11,
				12,13,14, 12,14,15,
				16,17,18, 16,18,19,
				20,21,22, 20,22,23,
			}, 0);
			mesh.SetNormals(new Vector3[] {
				Vector3.back, Vector3.back, Vector3.back, Vector3.back,
				Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
				Vector3.left, Vector3.left, Vector3.left, Vector3.left,
				Vector3.right, Vector3.right, Vector3.right, Vector3.right,
				Vector3.up, Vector3.up, Vector3.up, Vector3.up,
				Vector3.down, Vector3.down, Vector3.down, Vector3.down,
			});
		}

	}

	[System.Serializable]
	public class Section
	{
		public string TranslatePartName()
		{
			switch(partName)
			{
				case "gun": return "body";
				case "ammo": return "ammo_0";
				case "defaultScope": return "scope";
				case "defaultStock": return "stock";
				case "defaultGrip": return "grip";
				case "defaultBarrel": return "barrel";
				case "revolverBarrel": return "revolver";
				default: return partName;
			}
			
		}
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
