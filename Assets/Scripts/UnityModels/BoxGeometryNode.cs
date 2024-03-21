using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BoxGeometryNode : GeometryNode
{
	public Vector3 Pos { get { return transform.localPosition; } }
	public Vector3 Dim = Vector3.one;



	public Vector3Int DimsForUV { get { return new Vector3Int(Mathf.CeilToInt(Dim.x), Mathf.CeilToInt(Dim.y), Mathf.CeilToInt(Dim.z)); } }
	public override Vector2Int BoxUVBounds { get { return new Vector2Int(DimsForUV.z * 2 + DimsForUV.x * 2, DimsForUV.z + DimsForUV.y); } }
	public override UVPatch UVRequirements { get { return new BoxUVPatch() { Key = UniqueName, BoxDims = DimsForUV }; } }

	// Operations
	public override bool SupportsMirror() { return true; }
	public override void Mirror(bool mirrorX, bool mirrorY, bool mirrorZ) 
	{
		transform.localPosition = new Vector3(
			mirrorX ? -transform.localPosition.x - Dim.x : transform.localPosition.x,
			mirrorY ? -transform.localPosition.y - Dim.y : transform.localPosition.y,
			mirrorZ ? -transform.localPosition.z - Dim.z : transform.localPosition.z);
		// TODO: transform.localEulerAngles = 
		//Offsets = JavaModelImporter.MirrorOffsets(Offsets, mirrorX, mirrorY, mirrorZ);
	}
	public virtual void Resize(Vector3 newOrigin, Vector3 newDims)
	{
		Undo.RegisterCompleteObjectUndo(gameObject, $"Resize BoxGeometry {name} to {newDims} at {newOrigin}");

		Vector3 deltaMin = newOrigin - LocalOrigin;
		Vector3 deltaMax = (newOrigin + newDims) - (LocalOrigin + Dim);
		deltaMin = Quaternion.Inverse(transform.localRotation) * deltaMin;
		deltaMax = Quaternion.Inverse(transform.localRotation) * deltaMax;
		LocalOrigin += deltaMin;
		Dim += deltaMax - deltaMin;

		EditorUtility.SetDirty(gameObject);
	}
	public virtual void Resize(Vector3 newDims)
	{
		Undo.RegisterCompleteObjectUndo(gameObject, $"Resize BoxGeometry {name} to {newDims}");
		Dim = newDims;
		EditorUtility.SetDirty(gameObject);
	}

#if UNITY_EDITOR
	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		// Box specific operations
		GUILayout.BeginHorizontal();
		if(GUILayout.Button(FlanCustomButtons.BoxBoundsToolButton))
		{
			System.Type toolType = System.Type.GetType("BoxBoundsTool, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			if (toolType != null)
				UnityEditor.EditorTools.ToolManager.SetActiveTool(toolType);
		}
		Vector3 newDim = FlanStyles.CompactVector3Field("Dimensions", Dim);
		if(!newDim.Approximately(Dim))
		{
			//Resize(newDim);
		}
		GUILayout.EndHorizontal();
	}
#endif

	// -------------------------------------------------------------------
	#region Geometry Generation
	// -------------------------------------------------------------------

	public override void GenerateGeometry(Vector2Int texSize, Vector2Int withUV)
	{
		Mesh.SetVertices(GenerateVertsWithUV(GenerateVertsNoUV()));
		List<Vector2> uvs = new List<Vector2>();
		int textureX = texSize.x;
		int textureY = texSize.y;
		int textureU = withUV.x;
		int textureV = withUV.y;
		uvs.AddRange(GenerateUVsForFace(EFace.north, textureU, textureV));
		uvs.AddRange(GenerateUVsForFace(EFace.south, textureU, textureV));
		uvs.AddRange(GenerateUVsForFace(EFace.west, textureU, textureV));
		uvs.AddRange(GenerateUVsForFace(EFace.east, textureU, textureV));
		uvs.AddRange(GenerateUVsForFace(EFace.up, textureU, textureV));
		uvs.AddRange(GenerateUVsForFace(EFace.down, textureU, textureV));
		for (int i = 0; i < uvs.Count; i++)
		{
			uvs[i] = new Vector2(uvs[i].x / textureX, (textureY - uvs[i].y) / textureY);
		}
		Mesh.SetUVs(0, uvs);
		Mesh.SetTriangles(GenerateTrisWithUV(), 0);
		Mesh.SetNormals(GenerateNormalsWithUV());
		Mesh.RecalculateBounds();
	}

	public override JObject ExportGeometryNode(Vector2Int texSize, Vector2Int withUV)
	{
		JArray jVerts = new JArray();
		foreach (Vector3 v in GenerateVertsNoUV())
			jVerts.Add(v.ToJson());
		return new JObject()
		{
			["verts"] = jVerts,
			["eulerRotations"] = ExportEuler.ToJson(),
			["rotationOrigin"] = ExportOrigin.ToJson(),
			["faces"] = new JObject()
			{
				["north"] = ExportFaceUV(texSize, withUV, EFace.north),
				["east"] = ExportFaceUV(texSize, withUV, EFace.east),
				["south"] = ExportFaceUV(texSize, withUV, EFace.south),
				["west"] = ExportFaceUV(texSize, withUV, EFace.west),
				["up"] = ExportFaceUV(texSize, withUV, EFace.up),
				["down"] = ExportFaceUV(texSize, withUV, EFace.down),

			},
		};
	}
	private JObject ExportFaceUV(Vector2Int texSize, Vector2Int withUV, EFace face)
	{
		Vector2[] uvs = GenerateUVsForFace(face, withUV.x, withUV.y, texSize.x, texSize.y);
		return new JObject()
		{
			["uv"] = new JArray( // minX, minY, maxX, maxY
				Mathf.Min(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x),
				Mathf.Min(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y),
				Mathf.Max(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x),
				Mathf.Max(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y)
			),
			["rotation"] = (face == EFace.down ? 270 : 180),
			["texture"] = "#default",
		};
	}
	#endregion
	// -------------------------------------------------------------------


	// ------------------------------------------------------------------------------------------
	#region UV-Mapped Mesh
	// UV-Mapped mesh, with 4 vertices per (shape)box corner, so each has a unique UV mapping
	// ------------------------------------------------------------------------------------------
	public Vector3[] GenerateVertsWithUV(Vector3[] inputVerts)
	{
		return new Vector3[] {
			inputVerts[0], inputVerts[2], inputVerts[3], inputVerts[1],	// -z face
			inputVerts[5], inputVerts[7], inputVerts[6], inputVerts[4], // +z face
			inputVerts[4], inputVerts[6], inputVerts[2], inputVerts[0], // -x face
			inputVerts[1], inputVerts[3], inputVerts[7], inputVerts[5], // +x face
			inputVerts[7], inputVerts[3], inputVerts[2], inputVerts[6], // +y face
			inputVerts[5], inputVerts[4], inputVerts[0], inputVerts[1], // -y face
		};
	}
	public Vector3[] GenerateNormalsWithUV() { return NORMALS_WITH_UV; }
	public int[] GenerateTrisWithUV() { return TRIS_WITH_UV; }
	public Vector2[] GenerateUVsForFace(EFace face, int tu, int tv, int tx, int ty)
	{
		return GenerateUVsForFace(face, tu, tv, 1f / tx, 1f / ty);
	}
	public Vector2[] GenerateUVsForFace(EFace face, int tu, int tv, float oneOverTx = 1.0f, float oneOverTy = 1.0f)
	{
		float x = Mathf.Ceil(Dim.x);
		float y = Mathf.Ceil(Dim.y);
		float z = Mathf.Ceil(Dim.z);

		switch (face)
		{
			case EFace.north:
				return new Vector2[] {
					new Vector2(oneOverTx * (tu + x * 2 + z * 2),	oneOverTy * (tv + y + z)),
					new Vector2(oneOverTx * (tu + x * 2 + z * 2),	oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + x + z * 2),		oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + x + z * 2),		oneOverTy * (tv + y + z)),
				};
			case EFace.south:
				return new Vector2[] {
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv + y + z)),
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + z),				oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + z),				oneOverTy * (tv + y + z)),
				};
			case EFace.west:
				return new Vector2[] {
					new Vector2(oneOverTx * (tu + z),				oneOverTy * (tv + y + z)),
					new Vector2(oneOverTx * (tu + z),				oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu),					oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu),					oneOverTy * (tv + y + z)),
				};
			case EFace.east:
				return new Vector2[] {
					new Vector2(oneOverTx * (tu + x + 2 * z),		oneOverTy * (tv + y + z)),
					new Vector2(oneOverTx * (tu + x + 2 * z),		oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv + y + z)),
				};
			case EFace.up:
				return new Vector2[] {
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv)),
					new Vector2(oneOverTx * (tu + z),				oneOverTy * (tv)),
					new Vector2(oneOverTx * (tu + z),				oneOverTy * (tv + z)),
				};
			case EFace.down:
				return new Vector2[] {
					new Vector2(oneOverTx * (tu + x * 2 + z),		oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv + z)),
					new Vector2(oneOverTx * (tu + x + z),			oneOverTy * (tv)),
					new Vector2(oneOverTx * (tu + x * 2 + z),		oneOverTy * (tv)),
				};
			default:
				return new Vector2[4];
		}
	}
	public static readonly int[] TRIS_WITH_UV = new int[] {
		0,1,2, 0,2,3,
		4,5,6, 4,6,7,
		8,9,10, 8,10,11,
		12,13,14, 12,14,15,
		16,17,18, 16,18,19,
		20,21,22, 20,22,23,
	};
	public static readonly Vector3[] NORMALS_WITH_UV = new Vector3[] {
		Vector3.back, Vector3.back, Vector3.back, Vector3.back,
		Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
		Vector3.left, Vector3.left, Vector3.left, Vector3.left,
		Vector3.right, Vector3.right, Vector3.right, Vector3.right,
		Vector3.up, Vector3.up, Vector3.up, Vector3.up,
		Vector3.down, Vector3.down, Vector3.down, Vector3.down,
	};
	#endregion
	// ------------------------------------------------------------------------------------------


	// ------------------------------------------------------------------------------------------
	#region Non-UV Mapped Mesh
	// This mesh has fewer vertices, but is not UV mapped
	// ------------------------------------------------------------------------------------------
	public static readonly int[] NON_UV_TRIS = new int[] {
		0,2,3,  0,3,1, // -z 0, 2, 3, 1, 
		5,7,6,  5,6,4, // +z 5, 7, 6, 4,
		4,6,2,  4,2,0, // -x 4, 6, 2, 0,
		1,3,7,  1,7,5, // +x 1, 3, 7, 5,
		7,3,2,  7,2,6, // +y 7, 3, 2, 6,
		5,4,0,  5,0,1, // -y 5, 4, 0, 1 
	};
	public int[] GenerateTrisNoUV() { return NON_UV_TRIS; }
	public virtual Vector3[] GenerateVertsNoUV()
	{
		Vector3[] verts = new Vector3[8];
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				for (int z = 0; z < 2; z++)
				{
					int index = x + y * 2 + z * 4;
					verts[index] = Vector3.zero;
					if (x == 1) verts[index].x += Dim.x;
					if (y == 1) verts[index].y += Dim.y;
					if (z == 1) verts[index].z += Dim.z;
				}
			}
		}
		return verts;
	}
	#endregion
	// ------------------------------------------------------------------------------------------

	public override string ToString()
	{
		return $"Box[{LocalOrigin}x{Dim}]";
	}
}
