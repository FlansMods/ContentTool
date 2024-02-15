using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class GeometryNode : Node
{
	// If the BoxUV size of Dim is not equal to these UV's box size, then we need to regen
	public RectInt BakedUV = new RectInt();

	// Helpers to remap UVs
	public bool HasBakedUVs() { return BakedUV.size != Vector2Int.zero; }
	// Weird edge case if you change the shape in a way that should alter UVs, but keeps the UV bounds the same
	public bool IsUVMapCurrent() { return BakedUV.size == BoxUVBounds; }
	public Vector2Int BakedUVBounds { get { return BakedUV.size; } }
	public abstract Vector2Int BoxUVBounds { get; }



	// -------------------------------------------------------------------
	#region Weird wrappers for required components
	// -------------------------------------------------------------------
	[SerializeField, HideInInspector]
	private MeshRenderer _MR = null;
	[SerializeField, HideInInspector]
	private MeshFilter _MF = null;
	[SerializeField, HideInInspector]
	private Mesh _Mesh = null;

	protected MeshRenderer MR
	{
		get
		{
			if (_MR == null)
				_MR = GetComponent<MeshRenderer>();
			if (_MR == null)
				_MR = gameObject.AddComponent<MeshRenderer>();
			return _MR;
		}
	}
	protected MeshFilter MF
	{
		get
		{
			if (_MF == null)
				_MF = GetComponent<MeshFilter>();
			if (_MF == null)
				_MF = gameObject.AddComponent<MeshFilter>();
			return _MF;
		}
	}
	protected Mesh Mesh
	{
		get
		{
			if (_Mesh == null)
				_Mesh = new Mesh();
			MF.sharedMesh = _Mesh;
			return _Mesh;
		}
	}
	#endregion
	// -------------------------------------------------------------------




	// -------------------------------------------------------------------
#if UNITY_EDITOR
	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		GUILayout.BeginHorizontal();
		EditorGUI.BeginDisabledGroup(true);
		GUILayout.Label($"UV Co-ordinates");
		GUILayout.FlexibleSpace();
		GUILayout.Label($"U:");
		GUILayout.TextField($"{BakedUV.x}", GUILayout.MinWidth(64));
		GUILayout.Label($"V:");
		GUILayout.TextField($"{BakedUV.y}", GUILayout.MinWidth(64));
		EditorGUI.EndDisabledGroup();
		GUILayout.EndHorizontal();
	}
#endif
	// -------------------------------------------------------------------

	public override bool NeedsMaterialType(ETurboRenderMaterial renderType) { return renderType == ETurboRenderMaterial.Cutout; }
	public override void ApplyMaterial(ETurboRenderMaterial renderType, Material mat) 
	{
		MR.sharedMaterial = mat;
	}

	protected override void EditorUpdate()
	{
		base.EditorUpdate();
		if (Root != null && Root is TurboRootNode turboRoot && EditorUtility.IsDirty(this))
		{
			GenerateBakedGeometry(turboRoot.UVMapSize);
			if (MR == null)
				Debug.Log("MeshRenderer null");
			Mesh.RecalculateBounds();
		}
	}


	// -------------------------------------------------------------------
	#region Geometry Generation
	// -------------------------------------------------------------------
	public void GenerateTempGeometry(Vector2Int texSize)
	{
		GenerateGeometry(texSize, BoxUVBounds);
	}
	public void GenerateBakedGeometry(Vector2Int texSize)
	{
		GenerateGeometry(texSize, BakedUV.min);
	}
	public abstract void GenerateGeometry(Vector2Int texSize, Vector2Int withUV);
	public abstract JObject ExportGeometryNode(Vector2Int texSize, Vector2Int withUV);
	#endregion
	// -------------------------------------------------------------------
}
