using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ShapeboxBoundsHandle : ShapeboxHandle
{
	private enum HandleID
	{
		Face_Pos_X,
		Face_Neg_X,
		Face_Pos_Y,
		Face_Neg_Y,
		Face_Pos_Z,
		Face_Neg_Z,

		// Edge_Pos_X_Pos_Y,
		// Edge_Pos_X_Neg_Y,
		// Edge_Neg_X_Pos_Y,
		// Edge_Neg_X_Neg_Y,
		// Edge_Pos_Y_Pos_Z,
		// Edge_Pos_Y_Neg_Z,
		// Edge_Neg_Y_Pos_Z,
		// Edge_Neg_Y_Neg_Z,
		// Edge_Pos_Z_Pos_X,
		// Edge_Pos_Z_Neg_X,
		// Edge_Neg_Z_Pos_X,
		// Edge_Neg_Z_Neg_X,

		NUM_HANDLES
	}

	private static readonly int NUM_HANDLES = (int)HandleID.NUM_HANDLES;
	protected override int NumControlIDs { get { return NUM_HANDLES; } }

	protected override void DrawSubHandles(bool isInsideCameraBox)
	{
		// Handle stuff
		Vector3 min = Origin;
		Vector3 max = Origin + Dimensions;
	
		FaceHandles(ref min, ref max, isInsideCameraBox);
		//EdgeHandles(ref min, ref max, isInsideCameraBox);

		ApplyChangesWithSnapping(min, max);
	}

	private void FaceHandles(ref Vector3 min, ref Vector3 max, bool isCameraInsideBox)
	{
		Vector3 center = (min + max) * 0.5f;
		Vector3 handlePos, modified;

		// PosX
		handlePos = new Vector3(max.x, center.y, center.z);
		modified = Slider1D(ControlIDs[(int)HandleID.Face_Pos_X], handlePos, YAxis, ZAxis, 0.1f, isCameraInsideBox);
		max.x = Mathf.Max(min.x, modified.x);
		// NegX
		handlePos = new Vector3(min.x, center.y, center.z);
		modified = Slider1D(ControlIDs[(int)HandleID.Face_Neg_X], handlePos, YAxis, -ZAxis, 0.1f, isCameraInsideBox);
		min.x = Mathf.Min(max.x, modified.x);

		// PosY
		handlePos = new Vector3(center.x, max.y, center.z);
		modified = Slider1D(ControlIDs[(int)HandleID.Face_Pos_Y], handlePos, XAxis, -ZAxis, 0.1f, isCameraInsideBox);
		max.y = Mathf.Max(min.y, modified.y);
		// NegY
		handlePos = new Vector3(center.x, min.y, center.z);
		modified = Slider1D(ControlIDs[(int)HandleID.Face_Neg_Y], handlePos, XAxis, ZAxis, 0.1f, isCameraInsideBox);
		min.y = Mathf.Min(max.y, modified.y);

		// PosZ
		handlePos = new Vector3(center.x, center.y, max.z);
		modified = Slider1D(ControlIDs[(int)HandleID.Face_Pos_Z], handlePos, YAxis, -XAxis, 0.1f, isCameraInsideBox);
		max.z = Mathf.Max(min.z, modified.z);
		// NegZ
		handlePos = new Vector3(center.x, center.y, min.z);
		modified = Slider1D(ControlIDs[(int)HandleID.Face_Neg_Z], handlePos, YAxis, XAxis, 0.1f, isCameraInsideBox);
		min.z = Mathf.Min(max.z, modified.z);

	}

	/*
	private void EdgeHandles(ref Vector3 min, ref Vector3 max, bool isCameraInsideBox)
	{
		Vector3 center = (min + max) * 0.5f;
		Vector3 handlePos, modified;

		// ----------------------------------------
		// Y-Verticals
		// ----------------------------------------
		// PosZ, PosX
		handlePos = new Vector3(max.x, center.y, max.z);
		modified = Slider2D(ControlIDs[(int)HandleID.Edge_Pos_Z_Pos_X], handlePos, XAxis, ZAxis, isCameraInsideBox);
		max.x = Mathf.Max(min.x, modified.x);
		max.z = Mathf.Max(min.z, modified.z);
		// NegZ, PosX
		handlePos = new Vector3(max.x, center.y, min.z);
		modified = Slider2D(ControlIDs[(int)HandleID.Edge_Neg_Z_Pos_X], handlePos, XAxis, ZAxis, isCameraInsideBox);
		max.x = Mathf.Max(min.x, modified.x);
		min.z = Mathf.Min(max.z, modified.z);
		// PosZ, NegX
		handlePos = new Vector3(min.x, center.y, max.z);
		modified = Slider2D(ControlIDs[(int)HandleID.Edge_Pos_Z_Neg_X], handlePos, XAxis, ZAxis, isCameraInsideBox);
		min.x = Mathf.Min(max.x, modified.x);
		max.z = Mathf.Max(min.z, modified.z);
		// NegZ, NegX
		handlePos = new Vector3(min.x, center.y, min.z);
		modified = Slider2D(ControlIDs[(int)HandleID.Edge_Neg_Z_Neg_X], handlePos, XAxis, ZAxis, isCameraInsideBox);
		min.x = Mathf.Min(max.x, modified.x);
		min.z = Mathf.Min(max.z, modified.z);
		// ----------------------------------------
	}*/
}
