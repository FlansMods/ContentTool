using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[ExecuteInEditMode]
public class TurboPiecePreview : MinecraftModelPreview
{
	private TurboModelPreview _Parent = null;
	public TurboModelPreview Parent
	{
		get
		{
			if (_Parent == null)
				_Parent = GetComponentInParent<TurboModelPreview>();
			return _Parent;
		}
	}
	public TurboPiece Piece
	{
		get
		{
			if (Parent != null && Parent.Section != null)
				return Parent.Section.GetPiece(PartIndex);
			return null;
		}
	}
	public int PartIndex = 0;
	public override MinecraftModel GetModel()
	{
		return Parent?.GetModel();
	}
	public override MinecraftModelPreview GetParent() { return Parent; }
	public UVMap CurrentUVMap
	{
		get
		{
			return GetComponentInParent<ModelEditingRig>()?.GetPreviewUVMap();
		}
	}
	private static readonly int[] OldVertexOrder = new int[] {
		7, // V1 = +x, +y, +z
		6, // V2 = -x, +y, +z
		2, // V3 = -x, +y, -z
		3, // V4 = +x, +y, -z
		5, // V5 = +x, -y, +z
		4, // V6 = -x, -y, +z
		0, // V7 = -x, -y, -z
		1, // V8 = +x, -y, -z
	};

	private static readonly string[] VertexNames = new string[] {
		"Offset 1 (+x, +y, +z)",
		"Offset 2 (-x, +y, +z)",
		"Offset 3 (-x, +y, -z)",
		"Offset 4 (+x, +y, -z)",
		"Offset 5 (+x, -y, +z)",
		"Offset 6 (-x, -y, +z)",
		"Offset 7 (-x, -y, -z)",
		"Offset 8 (+x, -y, -z)",
	};

#if UNITY_EDITOR
	public override string Compact_Editor_Header()
	{
		if (Piece == null)
			return "Invalid Piece!";
		int numOffsets = Piece.NumOffsetVertices();
		if (numOffsets == 0)
			return $"Box {name} [{Piece.Dim.x}x{Piece.Dim.y}x{Piece.Dim.z}]";
		return $"ShapeBox {name} [{Piece.Dim.x}x{Piece.Dim.y}x{Piece.Dim.z}] + {numOffsets} Edits";
	}
	public override void Compact_Editor_GUI()
	{
		if (Piece == null)
			return;

		transform.localPosition = Piece.Origin;
		if (Piece.Offsets.Length != 8)
			Piece.Offsets = new Vector3[8];		

		// (Duplicate of Unity transform fields, but important)
		Vector3 newOrigin = FlanStyles.CompactVector3Field("Rotation Origin", Piece.Origin);
		Vector3 newEuler = FlanStyles.CompactVector3Field("Euler Angles", Piece.Euler);
		if (!newOrigin.Approximately(Piece.Origin)
		|| !newEuler.Approximately(Piece.Euler))
		{
			ModelEditingSystem.ApplyOperation(
				new TurboUnityTransformOperation(
					GetModel(),
					Parent.PartName,
					PartIndex,
					newOrigin,
					Quaternion.Euler(newEuler)));
		}

		// Box size and position
		Vector3 newPos = FlanStyles.CompactVector3Field("Cube Offset", Piece.Pos);
		Vector3 newDim = FlanStyles.CompactVector3Field("Dimensions", Piece.Dim);
		if(!newPos.Approximately(Piece.Pos)
		|| !newDim.Approximately(Piece.Dim))
		{
			ModelEditingSystem.ApplyOperation(
				new TurboResizeBoxOperation(
					GetModel(),
					Parent.PartName,
					PartIndex,
					new Bounds(newPos + newDim / 2, newDim)));
		}

		// Shapebox corner fields
		List<int> modifiedIndices = new List<int>();
		List<Vector3> modifedOffsets = new List<Vector3>();
		for (int i = 0; i < 8; i++)
		{
			int TransmutedVertexIndex = OldVertexOrder[i];
			Vector3 newOffset = FlanStyles.CompactVector3Field(VertexNames[i], Piece.Offsets[TransmutedVertexIndex]);
			if (!newOffset.Approximately(Piece.Offsets[TransmutedVertexIndex]))
			{
				modifiedIndices.Add(TransmutedVertexIndex);
				modifedOffsets.Add(newOffset);
			}
		}
		if(modifiedIndices.Count > 0)
		{
			ModelEditingSystem.ApplyOperation(
				new TurboEditOffsetsOperation(
					GetModel(),
					Parent.PartName,
					PartIndex,
					modifiedIndices,
					modifedOffsets));
		}
	}
	public override void Compact_Editor_Texture_GUI()
	{
		GUILayout.BeginHorizontal();

		GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 15));

		GUILayout.BeginVertical(GUILayout.Width(100));
		GUILayout.Label($"Box [{Piece.Dim.x}x{Piece.Dim.y}x{Piece.Dim.z}]");
		GUILayout.Label($"Tex [{Piece.BoxUVSize.x}x{Piece.BoxUVSize.y}]");
		GUILayout.EndVertical();

		UVMap map = CurrentUVMap;
		if(map != null)
		{
			BoxUVPlacement placement = map.GetPlacedPatch($"{Parent.PartName}/{PartIndex}");
			if(placement == null)
			{
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.BeginHorizontal();
				EditorGUILayout.IntField("Tex U:", 0);
				EditorGUILayout.IntField("Tex V:", 0);
				GUILayout.EndHorizontal();
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.BeginHorizontal();
				EditorGUILayout.IntField("Tex U:", placement.Origin.x);
				EditorGUILayout.IntField("Tex V:", placement.Origin.y);
				GUILayout.EndHorizontal();
				EditorGUI.EndDisabledGroup();
			}
		}
	}
	#endif
	public override bool CanDelete() { return true; }
	public override bool CanDuplicate() { return true; }
	public override ModelEditOperation Delete()
	{
		return new TurboDeletePieceOperation(GetModel(), Parent.PartName, PartIndex);
	}
	public override ModelEditOperation Duplicate()
	{
		return new TurboDuplicatePieceOperation(GetModel(), Parent.PartName, PartIndex);
	}

	protected override void EditorUpdate()
	{
		base.EditorUpdate();

		if (Piece == null)
			return;

		if (HasUnityTransformBeenChanged())
		{
			ModelEditingSystem.ApplyOperation(
				new TurboUnityTransformOperation(
					GetModel(),
					Parent.PartName,
					PartIndex,
					transform.localPosition,
					transform.localRotation));	
		}
		if(!ModelEditingSystem.ShouldSkipRefresh(GetModel(), Parent.PartName, PartIndex))
			CopyToUnityTransform();
	}

	private bool HasUnityTransformBeenChanged()
	{
		return !transform.localPosition.Approximately(Piece.Origin)
			|| !transform.localEulerAngles.Approximately(Piece.Euler);
	}

	public void CopyToUnityTransform()
	{
		transform.localPosition = Piece.Origin;
		transform.localEulerAngles = Piece.Euler;
	}

	public static readonly int[] NON_UV_TRIS = new int[] {
		0,2,3,  0,3,1, // -z 0, 2, 3, 1, 
		5,7,6,  5,6,4, // +z 5, 7, 6, 4,
		4,6,2,  4,2,0, // -x 4, 6, 2, 0,
		1,3,7,  1,7,5, // +x 1, 3, 7, 5,
		7,3,2,  7,2,6, // +y 7, 3, 2, 6,
		5,4,0,  5,0,1, // -y 5, 4, 0, 1 
	};
	public int[] GetTris() { return NON_UV_TRIS; }

	public static readonly int[] VERTS_WITH_UV = new int[] {
		0, 2, 3, 1,	 // -z face
		5, 7, 6, 4,	 // +z face
		4, 6, 2, 0,	 // -x face
		1, 3, 7, 5,	 // +x face
		7, 3, 2, 6,	 // +y face
		5, 4, 0, 1,	 // -y face
	};
	public Vector3[] GenerateVertsForUV(Vector3[] inputVerts)
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
	public static readonly int[] TRIS_WITH_UV = new int[] {
		0,1,2, 0,2,3,
		4,5,6, 4,6,7,
		8,9,10, 8,10,11,
		12,13,14, 12,14,15,
		16,17,18, 16,18,19,
		20,21,22, 20,22,23,
	};
	public int[] GenerateTrisForUV() { return TRIS_WITH_UV; }
	public static readonly Vector3[] NORMALS_WITH_UV = new Vector3[] {
		Vector3.back, Vector3.back, Vector3.back, Vector3.back,
		Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
		Vector3.left, Vector3.left, Vector3.left, Vector3.left,
		Vector3.right, Vector3.right, Vector3.right, Vector3.right,
		Vector3.up, Vector3.up, Vector3.up, Vector3.up,
		Vector3.down, Vector3.down, Vector3.down, Vector3.down,
	};
	public Vector3[] GenerateNormalsForUV() { return NORMALS_WITH_UV; }

	public override void RefreshGeometry()
	{
		if (Piece == null)
			return;

		BoxUVPlacement placement = CurrentUVMap.GetPlacedPatch($"{Parent.PartName}/{PartIndex}");
		int textureX = CurrentUVMap.MaxSize.x;
		int textureY = CurrentUVMap.MaxSize.y;
		int textureU = placement.Origin.x;
		int textureV = placement.Origin.y;
		Vector3[] v = Piece.GetVerts();
		Mesh.SetVertices(GenerateVertsForUV(v));
		List<Vector2> uvs = new List<Vector2>();
		uvs.AddRange(Piece.GetUVS(EFace.north, textureU, textureV));
		uvs.AddRange(Piece.GetUVS(EFace.south, textureU, textureV));
		uvs.AddRange(Piece.GetUVS(EFace.west, textureU, textureV));
		uvs.AddRange(Piece.GetUVS(EFace.east, textureU, textureV));
		uvs.AddRange(Piece.GetUVS(EFace.up, textureU, textureV));
		uvs.AddRange(Piece.GetUVS(EFace.down, textureU, textureV));
		for (int i = 0; i < uvs.Count; i++)
		{
			uvs[i] = new Vector2(uvs[i].x / textureX, (textureY - uvs[i].y) / textureY);
		}
		Mesh.SetUVs(0, uvs);
		Mesh.SetTriangles(GenerateTrisForUV(), 0);
		Mesh.SetNormals(GenerateNormalsForUV());

		MR.sharedMaterial = GetComponentInParent<ModelEditingRig>()?.SkinMaterial;
	}

	public void OnDrawGizmosSelected()
	{
		if (Piece == null)
			return;
		Vector3[] verts = Piece.GetVerts();
		Vector3 center = Vector3.zero;
		for (int i = 0; i < 8; i++)
			center += verts[i];
		center /= 8;

		for (int i = 0; i < 8; i++)
		{
			Vector3 outwardDir = (verts[i] - center).normalized;
			Gizmos.DrawLine(
				transform.TransformPoint(verts[i]),
				transform.TransformPoint(verts[i] + outwardDir * 1.0f));
			Gizmos.DrawIcon(transform.TransformPoint(verts[i] + outwardDir * 1.2f), $"Vertex_{OldVertexOrder[i] + 1}.png");
		}
	}
}