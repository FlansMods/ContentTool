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
	public int GetVertexIndex(int x, int y, int z) { return x + y * 2 + z * 4; }
	public Vector3 GetVertexPos(int x, int y, int z) { return GetVerts()[GetVertexIndex(x, y, z)]; }
	public void SetVertexPos(int x, int y, int z, Vector3 position)
	{
		position -= Origin;
		position -= new Vector3(x * Dimensions.x, y * Dimensions.y, z * Dimensions.z);
		Offsets[GetVertexIndex(x, y, z)] = position;
	}
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

		// update if changed
		if (changed)
		{
			float snapIncrement = 1.0f;
			if (Event.current.shift)
				snapIncrement = 0.25f;

			// TODO: Only apply snap to modified components
			for(int i = 0; i < Offsets.Length; i++)
			{
				Offsets[i] = Snap(Offsets[i], snapIncrement);
			}

			SetOriginAndDims(Snap(Origin, snapIncrement), Snap(Dimensions, snapIncrement));


			// determine which handle changed to apply any further modifications
			/*
			center = (max + min) * 0.5f;
			Dimensions = max - min;
			for (int i = 0, count = OffsetControlIDs.Length; i < count; ++i)
			{
				if (GUIUtility.hotControl == OffsetControlIDs[i])
					BoundsIncOffsets = OnHandleChanged((HandleDirection)i, BoundsIncOffsetsOnClick, BoundsIncOffsets);
			}
			*/
		}
	}
	protected abstract void DrawSubHandles(bool isInsideCameraBox);

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
