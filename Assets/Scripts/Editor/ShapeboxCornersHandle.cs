using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;

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

		Face_Pos_X,
		Face_Neg_X,
		Face_Pos_Y,
		Face_Neg_Y,
		Face_Pos_Z,
		Face_Neg_Z,

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
		SetVertexPosWithSnap(0, 0, 0, modified);
		// -x, -y, +z
		handlePos = GetVertexPos(0, 0, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Neg_X_Neg_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPosWithSnap(0, 0, 1, modified);

		// -x, +y, -z
		handlePos = GetVertexPos(0, 1, 0);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Neg_X_Pos_Y_Neg_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPosWithSnap(0, 1, 0, modified);
		// -x, +y, +z
		handlePos = GetVertexPos(0, 1, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Neg_X_Pos_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPosWithSnap(0, 1, 1, modified);

		// +x, -y, -z
		handlePos = GetVertexPos(1, 0, 0);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Neg_Y_Neg_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPosWithSnap(1, 0, 0, modified);
		// +x, -y, +z
		handlePos = GetVertexPos(1, 0, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Neg_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPosWithSnap(1, 0, 1, modified);

		// +x, +y, -z
		handlePos = GetVertexPos(1, 1, 0);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Pos_Y_Neg_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPosWithSnap(1, 1, 0, modified);
		// +x, +y, +z
		handlePos = GetVertexPos(1, 1, 1);
		modified = Slider3D(ControlIDs[(int)HandleID.Corner_Pos_X_Pos_Y_Pos_Z], handlePos, XAxis, ZAxis, HANDLE_SIZE, isInsideCameraBox);
		SetVertexPosWithSnap(1, 1, 1, modified);

		Handles.color = Color.green;
		HandleFace(Direction.north);
		HandleFace(Direction.south);
		HandleFace(Direction.east);
		HandleFace(Direction.west);
		HandleFace(Direction.up);
		HandleFace(Direction.down);


	}

	public int[][] VertexIndices = new int[][] {
		new int[] { 0, 1, 2, 3 }, // North = -z face
		new int[] { 4, 5, 6, 7 }, // South = +z face
		new int[] { 1, 3, 5, 7 }, // East = +x face
		new int[] { 0, 2, 4, 6 }, // West = -x face
		new int[] { 2, 3, 6, 7 }, // Up = +y face
		new int[] { 0, 1, 4, 5 }, // Down = -y face

	};

	private void HandleFace(Direction direction)
	{
		int[] vIndices = VertexIndices[(int)direction];
		Vector3[] positions = new Vector3[4];
		Vector3 avgPosition = Vector3.zero;
		for(int i = 0; i < 4; i++)
		{
			positions[i] = GetVertexPos(vIndices[i]);
			avgPosition += positions[i];
		}
		avgPosition /= 4;
		Vector3 modified = Slider1D(
			ControlIDs[(int)HandleID.Face_Pos_X + (int)direction],
			avgPosition, 
			direction.UAxis(), 
			direction.VAxis(), 
			0.1f, 
			false);
		float modifiedOnAxis = Vector3.Dot(modified - avgPosition, direction.Normal());
		if (modifiedOnAxis != 0.0f)
		{
			float snapIncrement = 1.0f;
			if (Event.current.shift)
				snapIncrement = 0.25f;

			for (int i = 0; i < 4; i++)
			{
				Offsets[vIndices[i]] += direction.Normal() * modifiedOnAxis;
				Offsets[vIndices[i]] = Snap1D(Offsets[vIndices[i]], snapIncrement, direction.Normal());
			}
		}
	}

	
}
