using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShapeboxCornersHandle : ShapeboxHandle
{
	private enum HandleID
	{
		Corner_Pos_X_Pos_Y_Pos_Z,
		Corner_Pos_X_Pos_Y_Neg_Z,
		Corner_Pos_X_Neg_Y_Pos_Z,
		Corner_Pos_X_Neg_Y_Neg_Z,
		Corner_Neg_X_Pos_Y_Pos_Z,
		Corner_Neg_X_Pos_Y_Neg_Z,
		Corner_Neg_X_Neg_Y_Pos_Z,
		Corner_Neg_X_Neg_Y_Neg_Z,

		NUM_HANDLES
	}
	private static readonly int NUM_HANDLES = (int)HandleID.NUM_HANDLES;
	
	protected override int NumControlIDs { get { return NUM_HANDLES; } }
	protected override void DrawSubHandles(bool isInsideCameraBox)
	{
		// Handle stuff
		Vector3 handlePos, modified;

		

		// -x, -y, -z
		handlePos = GetVertexPos(0, 0, 0);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Neg_X_Neg_Y_Neg_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(0, 0, 0, modified);
		// -x, -y, +z
		handlePos = GetVertexPos(0, 0, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Neg_X_Neg_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(0, 0, 1, modified);

		// -x, +y, -z
		handlePos = GetVertexPos(0, 1, 0);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Neg_X_Pos_Y_Neg_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(0, 1, 0, modified);
		// -x, +y, +z
		handlePos = GetVertexPos(0, 1, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Neg_X_Pos_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(0, 1, 1, modified);

		// +x, -y, -z
		handlePos = GetVertexPos(1, 0, 0);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Neg_Y_Neg_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(1, 0, 0, modified);
		// +x, -y, +z
		handlePos = GetVertexPos(1, 0, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Neg_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(1, 0, 1, modified);

		// +x, +y, -z
		handlePos = GetVertexPos(1, 1, 0);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Pos_Y_Neg_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(1, 1, 0, modified);
		// +x, +y, +z
		handlePos = GetVertexPos(1, 1, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Pos_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPos(1, 1, 1, modified);
	}

	
}
