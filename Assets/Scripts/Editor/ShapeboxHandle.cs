using Codice.Client.BaseCommands;
using Codice.Client.BaseCommands.BranchExplorer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Rendering;
using static QuickJSONBuilder;
using static UnityEditor.PlayerSettings;

public abstract class ShapeboxHandle : MinecraftBoundsHandle
{
	public const float HANDLE_SIZE = 0.25f;

	// Definitions
	public Vector3 Max {
		get { return center + Dimensions * 0.5f; }
	}
	public Vector3 Min {
		get { return center - Dimensions * 0.5f; }
	}
	public Vector3 Origin
	{
		get { return center - Dimensions * 0.5f; }
	}
	public Vector3 Dimensions;
	public Vector3[] Offsets = new Vector3[8];
	public void SetOriginAndDims(Vector3 origin, Vector3 dims)
	{
		center = origin + dims * 0.5f;
		Dimensions = dims;
	}
	public void SetCenterAndSize(Vector3 center, Vector3 size)
	{
		this.center = center;
		Dimensions = size;
	}
	public void SetMinAndMax(Vector3 min, Vector3 max)
	{
		center = (min + max) / 2f;
		Dimensions = max - min;
	}
	public void SetOffsets(Vector3[] offsets)
	{
		for (int i = 0; i < 8; i++)
			Offsets[i] = offsets[i];
	}
	public int GetVertexIndex(int x, int y, int z) { return x + y * 2 + z * 4; }
	public Vector3 GetVertexPos(int x, int y, int z) { return GetVerts()[GetVertexIndex(x, y, z)]; }
	public Vector3 GetVertexPos(int index) { return GetVerts()[index]; }

	public Vector3[] GetVerts() 
	{
		Vector3[] verts = new Vector3[8];
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				for (int z = 0; z < 2; z++)
				{
					int index = GetVertexIndex(x, y, z);
					verts[index] = Origin;
					if (x == 1) verts[index].x += Dimensions.x;
					if (y == 1) verts[index].y += Dimensions.y;
					if (z == 1) verts[index].z += Dimensions.z;

					verts[index] += Offsets[index];
				}
			}
		}

		return verts;
	}

	protected static Vector3 XAxis { get { return Vector3.right; } }
	protected static Vector3 YAxis { get { return Vector3.up; } }
	protected static Vector3 ZAxis { get { return Vector3.forward; } }

	protected int[] ControlIDs;
	protected abstract int NumControlIDs { get; }
	protected Bounds CurrentBounds;
	protected Bounds CachedBounds;

	private Material OutlineMaterial;

	private void BindOutlineMaterial()
	{
		if (OutlineMaterial == null)
		{
			OutlineMaterial = new Material(Shader.Find("Hidden/Internal-Colored")) { hideFlags = HideFlags.HideAndDontSave };
			OutlineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
			OutlineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
			OutlineMaterial.SetInt("_Cull", (int)CullMode.Off);
			OutlineMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
		}
		OutlineMaterial.SetPass(0);
	}

	// Wireframe, show the source cube, and any modified corners
	protected override void DrawWireframe()
	{
		//Handles.DrawWireCube(center, Dimensions);
	}
	public override void DrawMinecraftHandle()
	{
		// Acquire controlIDs
		if (ControlIDs == null)
			ControlIDs = new int[NumControlIDs];
		for (int i = 0; i < NumControlIDs; ++i)
			ControlIDs[i] = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

		// Draw wireframe outline
		using (new Handles.DrawingScope(Handles.color * wireframeColor))
		{
			Handles.DrawWireCube(center, Dimensions);


			BindOutlineMaterial();
			Vector3[] v = GetVerts();
			GL.PushMatrix();
			GL.MultMatrix(Handles.matrix);
			GL.Begin(GL.LINES);
			GL.Color(Color.magenta);

			GL.Vertex(v[0]); GL.Vertex(v[1]);
			GL.Vertex(v[2]); GL.Vertex(v[3]);
			GL.Vertex(v[4]); GL.Vertex(v[5]);
			GL.Vertex(v[6]); GL.Vertex(v[7]);

			GL.Vertex(v[0]); GL.Vertex(v[2]);
			GL.Vertex(v[1]); GL.Vertex(v[3]);
			GL.Vertex(v[4]); GL.Vertex(v[6]);
			GL.Vertex(v[5]); GL.Vertex(v[7]);

			GL.Vertex(v[0]); GL.Vertex(v[4]);
			GL.Vertex(v[1]); GL.Vertex(v[5]);
			GL.Vertex(v[2]); GL.Vertex(v[6]);
			GL.Vertex(v[3]); GL.Vertex(v[7]);

			GL.End();
			GL.PopMatrix();
		}

		if (ExitBecauseOfAlt())
			return;

		int previousHotControl = GUIUtility.hotControl;
		bool isInsideCameraBox = Camera.current != null && CurrentBounds.Contains(Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position));

		EditorGUI.BeginChangeCheck();
		using (new Handles.DrawingScope(Handles.color * handleColor))
		{
			if (Handles.color.a > 0.0f)
			{
			
				DrawSubHandles(isInsideCameraBox);
			}
		}
		bool changed = EditorGUI.EndChangeCheck();
		// detect if any handles got hotControl
		if (previousHotControl != GUIUtility.hotControl && GUIUtility.hotControl != 0)
		{
			CachedBounds = CurrentBounds;
		}
	}
	protected abstract void DrawSubHandles(bool isInsideCameraBox);

	protected void ApplyChangesWithSnapping(Vector3 min, Vector3 max)
	{
		float snapIncrement = 1.0f;
		if (Event.current.shift)
			snapIncrement = 0.25f;

		if (!Mathf.Approximately(min.x, Min.x))
			min.x = Snap(min.x, snapIncrement);
		if (!Mathf.Approximately(min.y, Min.y))
			min.y = Snap(min.y, snapIncrement);
		if (!Mathf.Approximately(min.z, Min.z))
			min.z = Snap(min.z, snapIncrement);
		if (!Mathf.Approximately(max.x, Max.x))
			max.x = Snap(max.x, snapIncrement);
		if (!Mathf.Approximately(max.y, Max.y))
			max.y = Snap(max.y, snapIncrement);
		if (!Mathf.Approximately(max.z, Max.z))
			max.z = Snap(max.z, snapIncrement);

		SetMinAndMax(min, max);
	}

	public void SetVertexPosWithSnap(int x, int y, int z, Vector3 position)
	{
		Vector3 oldPos = GetVertexPos(x, y, z);
		if ((position - oldPos).sqrMagnitude <= 0.00001f)
			return;

		float snapIncrement = 1.0f;
		if (Event.current.shift)
			snapIncrement = 0.25f;

		position -= Origin;
		position -= new Vector3(x * Dimensions.x, y * Dimensions.y, z * Dimensions.z);
		Offsets[GetVertexIndex(x, y, z)] = Snap(position, snapIncrement);
	}

	private bool ExitBecauseOfAlt()
	{
		// unless holding alt to pin center, exit before drawing control handles when holding alt, since alt-click will rotate scene view
		if (Event.current.alt)
		{
			bool exit = true;
			foreach (var id in ControlIDs)
			{
				if (id == GUIUtility.hotControl)
				{
					exit = false;
					break;
				}
			}
			return exit;
		}
		return false;
	}

}
