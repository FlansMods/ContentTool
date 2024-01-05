using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class AttachPointHandle : MinecraftBoundsHandle
{
	public const float HANDLE_SIZE = 0.25f;
	public Vector3 Origin 
	{ 
		get { return center; } 
		set { center = value; } 
	}
	public Vector3 Direction = Vector3.forward;
	public bool IsInfinite = false;
	public Color Colour = Color.black;
	protected int ControlID = -1;

	public void CopyFromAttachPoint(AttachPointNode apNode)
	{
		Origin = apNode.transform.localPosition;
		Direction = apNode.transform.forward;
		IsInfinite = apNode.name.Contains("eye_line");
		Colour = Color.black; // apNode.GetDebugColour();
	}

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

	public override void DrawMinecraftHandle()
	{
		if (ControlID == -1)
		{
			ControlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
		}

		using (new Handles.DrawingScope(Handles.color * Colour))
		{
			if (IsInfinite)
			{
				Handles.DrawLine(Origin - Direction * 100f, Origin + Direction * 100f);
			}
			else
			{
				Handles.DrawLine(Origin, Origin + Direction * 3.0f);
			}
			Handles.DrawWireCube(Origin + Direction * (1.5f), (Vector3.one + Direction * 6.0f) * HANDLE_SIZE);

			EditorGUI.BeginChangeCheck();
			Vector3 modified = Slider3D(ControlID, Origin, Vector3.forward, Vector3.right, HANDLE_SIZE, false);
			bool changed = EditorGUI.EndChangeCheck();

			if (changed)
			{
				float snapIncrement = 1.0f;
				if (Event.current.shift)
					snapIncrement = 0.25f;

				Vector3 delta = modified - Origin;
				if (!Mathf.Approximately(delta.x, 0))
					modified.x = Snap(modified.x, snapIncrement);
				if (!Mathf.Approximately(delta.y, 0))
					modified.y = Snap(modified.y, snapIncrement);
				if (!Mathf.Approximately(delta.z, 0))
					modified.z = Snap(modified.z, snapIncrement);
				Origin = modified;
			}
		}
	}

	protected override void DrawWireframe()
	{
		
	}
}
