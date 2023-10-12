using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Codice.Client.BaseCommands.Import.Commit;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class AttachPointHandle : MinecraftBoundsHandle
{
	public const float HANDLE_SIZE = 0.25f;
	public Vector3 Origin 
	{ 
		get { return center; } 
		set { center = value; } 
	}
	public Vector3 Direction = Vector3.forward;


	public Vector3[] Origins = new Vector3[0];
	public Vector3[] Directions = new Vector3[0];
	public Color[] Colours = new Color[0];
	protected int[] ControlIDs = new int[0];

	public void CopyFromAttachPoints(List<AttachPoint> points)
	{
		Origins = new Vector3[points.Count];
		Directions = new Vector3[points.Count];
		Colours = new Color[points.Count];
		for(int i = 0; i < points.Count; i++)
		{
			Origins[i] = points[i].position;
			Directions[i] = points[i].GuessDirection();
			Colours[i] = points[i].GetDebugColour();
		}
	}

	public void CopyToAttachPoints(List<AttachPoint> points)
	{
		if (Origins.Length != points.Count)
			return;
		for (int i = 0; i < points.Count; i++)
		{
			points[i].position = Origins[i];
		}
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
		if (ControlIDs.Length != Origins.Length)
		{
			ControlIDs = new int[Origins.Length];
			for (int i = 0; i < ControlIDs.Length; i++)
				ControlIDs[i] = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
		}

		for (int i = 0; i < Origins.Length; i++)
		{
			using (new Handles.DrawingScope(Handles.color * Colours[i]))
			{
				Handles.DrawLine(Origins[i], Origins[i] + Directions[i] * 3.0f);
				Handles.DrawWireCube(Origins[i] + Directions[i] * (1.5f), (Vector3.one + Directions[i] * 6.0f) * HANDLE_SIZE);

				EditorGUI.BeginChangeCheck();
				Origins[i] = Slider3D(ControlIDs[i], Origins[i], Vector3.forward, Vector3.right, HANDLE_SIZE, false);
				bool changed = EditorGUI.EndChangeCheck();

				if (changed)
				{
					float snapIncrement = 1.0f;
					if (Event.current.shift)
						snapIncrement = 0.25f;
					Origins[i] = Snap(Origins[i], snapIncrement);
				}
			}
		}
	}

	protected override void DrawWireframe()
	{
		
	}
}
