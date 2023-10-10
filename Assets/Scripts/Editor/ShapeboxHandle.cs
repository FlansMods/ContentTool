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
	private Vector3 Snap(Vector3 input, float snapIncrement)
	{
		return new Vector3(
					Mathf.Round(input.x / snapIncrement) * snapIncrement,
					Mathf.Round(input.y / snapIncrement) * snapIncrement,
					Mathf.Round(input.z / snapIncrement) * snapIncrement);
	}

	private static MethodInfo Func_Slider1D_Do = typeof(EditorWindow).Assembly
		.GetType("UnityEditorInternal.Slider1D")
		.GetMethod("Do",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
			null,
			new Type[] {
				typeof(int),
				typeof(Vector3),
				typeof(Vector3),
				typeof(float),
				typeof(Handles.CapFunction),
				typeof(float)
			},
			new ParameterModifier[6]);

	private static MethodInfo Func_Slider2D_Do = typeof(EditorWindow).Assembly
		.GetType("UnityEditorInternal.Slider2D")
		.GetMethod("Do",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
			null,
			new Type[] {
				typeof(int),
				typeof(Vector3),
				typeof(Vector3),
				typeof(Vector3),
				typeof(Vector3),
				typeof(float),
				typeof(Handles.CapFunction),
				typeof(float),
				typeof(bool)
			},
			new ParameterModifier[9]);

	public Vector3 Slider1D(int id, Vector3 localPos, Vector3 localTangent, Vector3 localBinormal, float size, bool isCameraInsideBox)
	{
		if (Handles.color.a > 0f && midpointHandleDrawFunction != null)
		{
			Handles.CapFunction capFunction = (a, b, c, d, e) => { DrawCap1D(a, b, c, d, e, localTangent, localBinormal); };
			Vector3 localDir = Vector3.Cross(localTangent, localBinormal).normalized;
			Vector3 modifiedPos = (Vector3)Func_Slider1D_Do.Invoke(null, new object[] {
				id,
				localPos,
				localDir,
				size,
				capFunction,
				EditorSnapSettings.scale
			});
			localPos = modifiedPos;
		}
		return localPos;
	}

	public Vector3 Slider2D(int id, Vector3 localPos, Vector3 localTangent, Vector3 localBinormal, float size, bool isCameraInsideBox)
	{
		if (Handles.color.a > 0f && midpointHandleDrawFunction != null)
		{
			Vector3 localDir = Vector3.Cross(localTangent, localBinormal).normalized;
			//var size = midpointHandleSizeFunction == null ? 0f : midpointHandleSizeFunction(localPos);
			Handles.CapFunction capFunction = (a, b, c, d, e) => { DrawCap2D(a, b, c, d, e, localTangent, localBinormal); };
			Vector3 modifiedPos = (Vector3)Func_Slider2D_Do.Invoke(null, new object[] {
				id,
				localPos,
				localDir,
				localTangent,
				localBinormal,
				size,
				capFunction,
				EditorSnapSettings.scale,
				true
			});
			localPos = modifiedPos;
		}
		return localPos;
	}

	public Vector3 Slider3D(int id, Vector3 localPos, Vector3 localUAxis, Vector3 localVAxis, float size, bool isCameraInsideBox)
	{
		if(Event.current.control)
		{
			Vector3 wAxis = Vector3.Cross(localUAxis, localVAxis);
			Vector3 newVAxis = Vector3.Cross(wAxis, localUAxis);
			Vector3 newUAxis = Vector3.Cross(wAxis, localVAxis);
			float newVDot = Vector3.Dot(Camera.current.transform.forward, newVAxis);
			float newUDot = Vector3.Dot(Camera.current.transform.forward, newUAxis);
			if (Mathf.Abs(newVDot) < Mathf.Abs(newUDot))
				return Slider2D(id, localPos, localVAxis, wAxis, size, isCameraInsideBox);
			else
				return Slider2D(id, localPos, localUAxis, wAxis, size, isCameraInsideBox);
		}
		else
		{
			return Slider2D(id, localPos, localUAxis, localVAxis, size, isCameraInsideBox);
		}
	}

	public void DrawCap1D(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType, Vector3 uAxis, Vector3 vAxis)
	{
		if (controlID == GUIUtility.hotControl)
		{
			int gridSize = Event.current.shift ? 16 : 4;
			float gridScale = Event.current.shift ? 0.25f : 1.0f;

			Vector3 wAxis = Vector3.Cross(uAxis, vAxis);
			float uComponent = Vector3.Dot(position, uAxis);
			float vComponent = Vector3.Dot(position, vAxis);
			float wComponent = Vector3.Dot(position, wAxis);

			Vector3 centerPoint =
				  uComponent * uAxis
				+ vComponent * vAxis
				+ Mathf.Round(wComponent / (gridSize * gridScale)) * (gridSize * gridScale) * wAxis;

			Handles.DrawDottedLine(
				centerPoint + gridScale * (-wAxis * gridSize),
				centerPoint + gridScale * (wAxis * gridSize), 0.1f);

			for (int i = -gridSize; i <= gridSize; i++)
			{
				float width = Event.current.shift && i % 4 == 0 ? 4.0f : 1.0f;
				
				Handles.DrawDottedLine(
					centerPoint + gridScale * (wAxis * i - uAxis * width),
					centerPoint + gridScale * (wAxis * i + uAxis * width), 0.1f);
			}
		}

		if (midpointHandleDrawFunction != null)
		{
			midpointHandleDrawFunction(controlID, position, rotation, size, eventType);
		}
	}

	public void DrawCap2D(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType, Vector3 uAxis, Vector3 vAxis)
	{
		if (controlID == GUIUtility.hotControl)
		{
			int gridSize = Event.current.shift ? 16 : 4;
			float gridScale = Event.current.shift ? 0.25f : 1.0f;

			Vector3 wAxis = Vector3.Cross(uAxis, vAxis);
			float uComponent = Vector3.Dot(position, uAxis);
			float vComponent = Vector3.Dot(position, vAxis);
			float wComponent = Vector3.Dot(position, wAxis);

			Vector3 centerPoint =
				  Mathf.Round(uComponent / (gridSize * gridScale)) * (gridSize * gridScale) * uAxis
				+ Mathf.Round(vComponent / (gridSize * gridScale)) * (gridSize * gridScale) * vAxis
				+ wComponent * wAxis;
			
			for(int i = -gridSize; i <= gridSize; i++)
			{
				Handles.DrawDottedLine(
					centerPoint + gridScale * (uAxis * i - vAxis * gridSize),
					centerPoint + gridScale * (uAxis * i + vAxis * gridSize), 0.1f);
				Handles.DrawDottedLine(								   
					centerPoint + gridScale * (vAxis * i - uAxis * gridSize),
					centerPoint + gridScale * (vAxis * i + uAxis * gridSize), 0.1f);
			}
		}

		if (midpointHandleDrawFunction != null)
		{
			midpointHandleDrawFunction(controlID, position, rotation, size, eventType);
		}
	}

	/*
	 * private void AdjustMidpointHandleColor(Vector3 localPos, Vector3 localTangent, Vector3 localBinormal, bool isCameraInsideBox)
	{
		float alphaMultiplier = 1f;

		// if inside the box then ignore back facing alpha multiplier (otherwise all handles will look disabled)
		if (!isCameraInsideBox && axes == (Axes.X | Axes.Y | Axes.Z))
		{
			// use tangent and binormal to calculate normal in case handle matrix is skewed
			Vector3 worldTangent = Handles.matrix.MultiplyVector(localTangent);
			Vector3 worldBinormal = Handles.matrix.MultiplyVector(localBinormal);
			Vector3 worldDir = Vector3.Cross(worldTangent, worldBinormal).normalized;

			// adjust color if handle is back facing
			float cosV;

			if (Camera.current.orthographic)
				cosV = Vector3.Dot(-Camera.current.transform.forward, worldDir);
			else
				cosV = Vector3.Dot((Camera.current.transform.position - Handles.matrix.MultiplyPoint(localPos)).normalized, worldDir);

			if (cosV < -0.0001f)
				alphaMultiplier *= 0.5f; // Handles.backfaceAlphaMultiplier;
		}

		Handles.color *= new Color(1f, 1f, 1f, alphaMultiplier);
	}
	 */

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
