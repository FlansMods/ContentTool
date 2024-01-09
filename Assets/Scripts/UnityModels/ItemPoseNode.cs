using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemPoseNode : Node
{
	public override string GetFixedPrefix() { return "pose_"; }
	public string PoseName { get { return name.Substring("pose_".Length); } }

	public override bool SupportsPreview() { return true; }
	public override void Preview() 
	{
		
	}

	public ItemDisplayContext TransformType = ItemDisplayContext.FIXED;

#if UNITY_EDITOR
	public override bool HasCompactEditorGUI() { return true; }
	public override void CompactEditorGUI()
	{
		base.CompactEditorGUI();

		ItemDisplayContext changedContext = (ItemDisplayContext)EditorGUILayout.EnumPopup(TransformType);
		if(changedContext != TransformType)
		{
			Undo.RecordObject(this, $"Selected TransformType: {changedContext}");
			TransformType = changedContext;
			name = $"pose_{TransformType}";
			EditorUtility.SetDirty(this);
		}
	}
#endif
}
