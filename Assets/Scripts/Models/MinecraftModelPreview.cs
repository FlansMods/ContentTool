using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public abstract class MinecraftModelPreview : MonoBehaviour
{
	public static float TextureZoomLevel = 2;
	protected MinecraftModel Model = null;
	public void SetModel(MinecraftModel model)
	{
		Model = model;
	}
	public virtual MinecraftModel GetModel()
	{
		return Model;
	}
	// -------------------------------------------------------------------
	#region Weird wrappers for required components
	// -------------------------------------------------------------------
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
	[SerializeField]
	private MeshRenderer _MR = null;
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
	[SerializeField]
	private MeshFilter _MF = null;
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
	[SerializeField]
	private Mesh _Mesh = null;
	#endregion
	// -------------------------------------------------------------------

	public float SnapSetting = 0.25f;

	protected virtual void OnEnable()
	{
		EditorApplication.update += EditorUpdate;
	}

	protected virtual void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
	}

	protected virtual void EditorUpdate()
	{
		
	}

	public virtual void InitializePreviews() { }
	public virtual string Compact_Editor_Header() { return name; }
	public virtual void Compact_Editor_GUI() { }
	public virtual void Compact_Editor_Texture_GUI() { }
	public virtual MinecraftModelPreview GetParent() { return null; }
	public virtual IEnumerable<MinecraftModelPreview> GetChildren() { yield break; }
	public virtual bool CanDelete() { return false; }
	public virtual bool CanDuplicate() { return false; }
	public virtual bool CanAdd() { return false; }
	public virtual ModelEditOperation Delete() { return null; }
	public virtual ModelEditOperation Duplicate() { return null; }
	public virtual ModelEditOperation Add() { return null; }
	public virtual void RefreshGeometry() { }
}
