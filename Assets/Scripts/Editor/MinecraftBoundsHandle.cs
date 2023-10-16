using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public abstract class MinecraftBoundsHandle : PrimitiveBoundsHandle
{
	public abstract void DrawMinecraftHandle();


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
		if (Event.current.control)
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

			for (int i = -gridSize; i <= gridSize; i++)
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

	protected float Snap(float input, float snapIncrement)
	{
		return Mathf.Round(input / snapIncrement) * snapIncrement;
	}

	protected Vector3 Snap1D(Vector3 input, float snapIncrement, Vector3 snapDirection)
	{
		float component = Vector3.Dot(input, snapDirection);
		Vector3 remainder = input - (snapDirection * component);
		return remainder + snapDirection * Snap(component, snapIncrement);
	}

	protected Vector3 Snap(Vector3 input, float snapIncrement)
	{
		return new Vector3(
					Mathf.Round(input.x / snapIncrement) * snapIncrement,
					Mathf.Round(input.y / snapIncrement) * snapIncrement,
					Mathf.Round(input.z / snapIncrement) * snapIncrement);
	}
}