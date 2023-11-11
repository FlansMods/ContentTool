using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEngine.UI.Image;

[EditorTool("Attach Points", typeof(ModelEditingRig))]
public class TurboModelEditingRigAttachPointsTool : TurboAttachPointsTool
{
	protected override TurboRigPreview Rig { get { return (target as ModelEditingRig).Preview as TurboRigPreview; } }
}

[EditorTool("Attach Points", typeof(TurboRigPreview))]
public class TurboRigAttachPointsTool : TurboAttachPointsTool
{
	protected override TurboRigPreview Rig { get { return (target as TurboRigPreview); } }
}

[EditorTool("Attach Points", typeof(TurboAttachPointPreview))]
public class TurboAPAttachPointsTool : TurboAttachPointsTool
{
	protected override TurboRigPreview Rig { get { return (target as TurboAttachPointPreview).GetComponentInParent<TurboRigPreview>(); } }
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
	public override GUIContent toolbarIcon
	{
		get
		{
			GUIContent guiContent = new GUIContent(ToolbarIcon);
			guiContent.tooltip = "Edit Attach Points";
			return guiContent;
		}
	}
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
		if(Rig != null && Rig.Rig != null)
			Handle.CopyFromAttachPoints(Rig.Rig.AttachPoints);
	}

	private void CopyFromHandle()
	{
		if (Rig != null && Rig.Rig != null)
		{
			if (Handle.Origins.Length != Rig.Rig.AttachPoints.Count)
				return;
			for (int i = 0; i < Rig.Rig.AttachPoints.Count; i++)
			{
				if (!Rig.Rig.AttachPoints[i].position.Approximately(Handle.Origins[i]))
				{
					TurboAttachPointPreview apPreview = Rig.GetAPPreview(Rig.Rig.AttachPoints[i].name);
					ModelEditingSystem.ApplyOperation(
						new TurboAttachPointMoveOperation(
							Rig.Rig,
							Rig.Rig.AttachPoints[i].name,
							Handle.Origins[i],
							apPreview != null && apPreview.LockPartPositions,
							apPreview != null && apPreview.LockAttachPoints));
				}
			}
		}
			//Handle.CopyToAttachPoints(Rig.Rig.AttachPoints);
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
				Undo.RecordObject(rig.Rig, "Edit attach point");
				CopyFromHandle();
			}
		}
	}
}
