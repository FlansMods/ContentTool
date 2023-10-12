using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[EditorTool("Attach Points", typeof(TurboRigPreview))]
public class TurboRigAttachPointsTool : TurboAttachPointsTool
{
	protected override TurboRigPreview Rig { get { return (target as TurboRigPreview); } }
}

[EditorTool("Attach Points", typeof(TurboModelPreview))]
public class TurboModelAttachPointsTool : TurboAttachPointsTool
{
	protected override TurboRigPreview Rig { get { return (target as TurboModelPreview).GetComponentInParent<TurboRigPreview>(); } }
}

[EditorTool("Attach Points", typeof(TurboPiecePreview))]
public class TurboPieceAttachPointsTool : TurboAttachPointsTool
{
	protected override TurboRigPreview Rig { get { return (target as TurboPiecePreview).GetComponentInParent<TurboRigPreview>(); } }
}

public abstract class TurboAttachPointsTool : EditorTool
{
	private AttachPointHandle Handle = new AttachPointHandle();
	protected abstract TurboRigPreview Rig { get; }
	public override GUIContent toolbarIcon => new GUIContent(ToolbarIcon);
	public Texture2D ToolbarIcon = null;
	public void OnEnable()
	{
		ToolbarIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorAssets/attach_points.png");
	}
	public void OnDisable()
	{
		ToolbarIcon = null;
	}

	private void CopyToHandle()
	{
		Handle.CopyFromAttachPoints(Rig.Rig.AttachPoints);
	}

	private void CopyFromHandle()
	{
		Handle.CopyToAttachPoints(Rig.Rig.AttachPoints);
	}

	public override void OnToolGUI(EditorWindow window)
	{
		TurboRigPreview rig = Rig;
		if (rig == null)
			return;

		if (Mathf.Approximately(rig.transform.lossyScale.sqrMagnitude, 0f))
			return;
		using (new Handles.DrawingScope(Matrix4x4.TRS(rig.transform.position, rig.transform.rotation, Vector3.one)))
		{
			CopyToHandle();
			EditorGUI.BeginChangeCheck();
			Handle.DrawMinecraftHandle();
			if (EditorGUI.EndChangeCheck())
			{
				CopyFromHandle();
			}
		}
	}
}
