using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttachPointNode : Node
{
	public override string GetFixedPrefix() { return "ap_"; }
	public string APName { get { return name.Substring("ap_".Length); } }
	//public EAttachmentType APType { get { } }
	public string AttachedTo { get { return ParentNode is AttachPointNode apNode ? apNode.APName : "body"; } }
	public Vector3 APOffset { get { return transform.localPosition; } }
	public Vector3 APEulers { get { return transform.localEulerAngles; } }



	// -------------------------------------------------------------------
#if UNITY_EDITOR
	public static bool LockPositionOfChildren = false;

	public override void Translate(Vector3 deltaPos)
	{
		if (LockPositionOfChildren)
		{
			deltaPos = PosSnap.Snap(deltaPos);
			if (!deltaPos.Approximately(Vector3.zero))
			{
				Undo.RegisterCompleteObjectUndo(gameObject, $"Offset {name} by {deltaPos} (keeping children fixed)");
				transform.TranslateButNotChildren(deltaPos);
				EditorUtility.SetDirty(gameObject);
			}
		}
		else base.Translate(deltaPos);
	}

	public override void Rotate(Vector3 deltaEuler)
	{
		if (LockPositionOfChildren)
		{
			deltaEuler = RotSnap.SnapEulers(deltaEuler);
			if (!deltaEuler.Approximately(Vector3.zero))
			{
				Undo.RegisterCompleteObjectUndo(gameObject, $"Added euler[{deltaEuler}] to {name} (keeping children fixed)");
				transform.RotateButNotChildren(deltaEuler);
				EditorUtility.SetDirty(gameObject);
			}
		}
		else base.Translate(deltaEuler);
	}

	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		LockPositionOfChildren = GUILayout.Toggle(LockPositionOfChildren, "Lock Children (APs and Models) when Transforming");	
	}
#endif
	// -------------------------------------------------------------------

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		//Gizmos.DrawLine(ap.transform.position, transform.position);

		Gizmos.matrix = transform.localToWorldMatrix;

		string rootPartName = APName;
		if (rootPartName.Contains("_"))
		{
			string lastPart = rootPartName.Substring(rootPartName.LastIndexOf("_") + 1);
			if (int.TryParse(lastPart, out int partIndex))
				rootPartName = rootPartName.Substring(0, rootPartName.LastIndexOf("_"));

		}

		switch (rootPartName)
		{
			case "eye_line":
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
				Gizmos.DrawLine(-Minecraft.Forward * 10f, Minecraft.Forward * 100f);
				Gizmos.DrawSphere(-Minecraft.Forward * 10f, 0.5f);
				Gizmos.DrawWireCube(-Minecraft.Forward * 10f, new Vector3(5.0f, 3.0f, 0.5f));
				break;
			case "shoot_origin":
				Gizmos.color = Color.red;
				Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
				Gizmos.DrawLine(Vector3.zero, Minecraft.Forward * 10f);
				Gizmos.DrawCube(Vector3.zero + Minecraft.Forward * 5f, Vector3.one * 0.7f);
				Gizmos.DrawCube(Vector3.zero + Minecraft.Forward * 10f, Vector3.one * 1.2f);
				break;
			case "laser_origin":
				Gizmos.color = Color.red;
				Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
				Gizmos.DrawLine(Vector3.zero, Minecraft.Forward * 100f);
				break;
			case "sights":
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
				Gizmos.DrawLine(Vector3.zero, Minecraft.Up * 5.0f);
				Gizmos.DrawWireCube(Minecraft.Up * 1.5f, new Vector3(2f, 2f, 6f));
				break;
			case "grip":
				Gizmos.color = Color.yellow;
				Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
				Gizmos.DrawLine(Vector3.zero, Minecraft.Down * 5.0f);
				Gizmos.DrawWireCube(Minecraft.Down * 0.5f, new Vector3(2f, 1f, 4f));
				Gizmos.DrawWireCube(Minecraft.Down * 3f, new Vector3(1.5f, 4f, 1.5f));
				break;
			case "barrel":
				Gizmos.color = Color.white;
				Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
				Gizmos.DrawLine(Vector3.zero, Minecraft.Forward * 5.0f);
				Gizmos.DrawWireCube(Minecraft.Forward * 2f, new Vector3(1f, 1f, 4f));
				break;
			default:
				Gizmos.color = Color.green;
				Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.4f);
				break;
		}
	}
}
