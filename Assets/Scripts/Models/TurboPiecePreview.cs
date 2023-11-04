using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
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

		EditorGUI.BeginDisabledGroup(true);
		GUILayout.BeginHorizontal();
		EditorGUILayout.IntField("Tex U:", Piece.textureU);
		EditorGUILayout.IntField("Tex V:", Piece.textureV);
		GUILayout.EndHorizontal();
		EditorGUI.EndDisabledGroup();

		// Basic box settings
		Vector3 newOrigin = FlanStyles.CompactVector3Field("Rotation Origin", transform.localPosition);
		Vector3 newEuler = FlanStyles.CompactVector3Field("Euler Angles", transform.localEulerAngles);
		Vector3 newPos = FlanStyles.CompactVector3Field("Cube Offset", Piece.Pos);
		Vector3 newDim = FlanStyles.CompactVector3Field("Dimensions", Piece.Dim);
		if(!newOrigin.Approximately(transform.localPosition)
		|| !newEuler.Approximately(transform.localEulerAngles)
		|| !newPos.Approximately(Piece.Pos)
		|| !newDim.Approximately(Piece.Dim))
		{
			Undo.RecordObject(Parent.Parent.Rig, "Modified Box transform/size");
			SetOrigin(newOrigin);
			SetEuler(newEuler);
			SetPos(newPos);
			SetDim(newDim);
			EditorUtility.SetDirty(Parent.Parent.Rig);
		}

		// Shapebox corner fields
		Vector3[] newOffsets = new Vector3[8];
		bool anyOffsetChanged = false;
		for (int i = 0; i < 8; i++)
		{
			int TransmutedVertexIndex = OldVertexOrder[i];
			newOffsets[TransmutedVertexIndex] = FlanStyles.CompactVector3Field(VertexNames[i], Piece.Offsets[TransmutedVertexIndex]);
			if (!newOffsets[TransmutedVertexIndex].Approximately(Piece.Offsets[TransmutedVertexIndex]))
				anyOffsetChanged = true;
		}
		if(anyOffsetChanged)
		{
			Undo.RecordObject(Parent.Parent.Rig, "Modified ShapeBox corners");
			for(int i = 0; i < 8; i++)
			{
				Piece.Offsets[i] = newOffsets[i];
			}
			EditorUtility.SetDirty(Parent.Parent.Rig);
		}

		
		
	}
	public override void Compact_Editor_Texture_GUI()
	{
		GUILayout.BeginHorizontal();

		GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 15));

		GUILayout.BeginVertical(GUILayout.Width(100));
		GUILayout.Label($"Box [{Piece.Dim.x}x{Piece.Dim.y}x{Piece.Dim.z}]");
		GUILayout.Label($"Tex [{Piece.GetBoxUVSize().x}x{Piece.GetBoxUVSize().y}]");
		GUILayout.EndVertical();

		// Texture
		//Texture2D tex = GetTemporaryTexture();
		//GUILayout.Label("", GUILayout.Width(tex.width * TextureZoomLevel), GUILayout.Height(tex.height * TextureZoomLevel));
		//GUI.DrawTexture(GUILayoutUtility.GetLastRect(), tex);
		//GUILayout.EndHorizontal();
	}
	#endif
	public override bool CanDelete() { return true; }
	public override bool CanDuplicate() { return true; }
	public override void Delete()
	{
		Parent?.DeleteChild(PartIndex);
	}
	public override void Duplicate()
	{
		Parent?.DuplicateChild(PartIndex);
	}

	protected override void Update()
	{
		base.Update();
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

		for(int i = 0; i < 8; i++)
		{
			Vector3 outwardDir = (verts[i] - center).normalized;
			Gizmos.DrawLine(
				transform.TransformPoint(verts[i]),
				transform.TransformPoint(verts[i] + outwardDir * 1.0f));
			Gizmos.DrawIcon(transform.TransformPoint(verts[i] + outwardDir * 1.2f), $"Vertex_{OldVertexOrder[i] + 1}.png");

		}
	}


	public override Vector3 SetOrigin(Vector3 localPos)
	{
		Vector3 resultPos = base.SetOrigin(localPos);
		if (Piece != null)
			Piece.Origin = resultPos;
		return resultPos;
	}

	public override Vector3 SetEuler(Vector3 localEuler)
	{
		Vector3 resultEuler = base.SetEuler(localEuler);
		if (Piece != null)
			Piece.Euler = resultEuler;
		return resultEuler;
	}

	public void SetPos(Vector3 pos)
	{
		if(Piece != null)
			Piece.Pos = pos;	
	}

	public void SetDim(Vector3 dim)
	{
		if (Piece != null)
			Piece.Dim = dim;
	}

	public override void GenerateMesh()
	{
		if (Piece == null)
			return;

		int textureX = Parent.Parent.Rig.TextureX;
		int textureY = Parent.Parent.Rig.TextureY;
		Piece.ExportToMesh(Mesh, textureX, textureY);

		MR.sharedMaterial = GetComponentInParent<ModelEditingRig>()?.GetSkinMaterial();

		transform.localPosition = Piece.Origin;
		transform.localEulerAngles = Piece.Euler;
	}
}