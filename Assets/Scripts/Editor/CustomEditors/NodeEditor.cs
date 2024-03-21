using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxGeometryNode))]
public class BoxGeometryNodeEditor : NodeEditor<BoxGeometryNode>
{

}
[CustomEditor(typeof(ShapeboxGeometryNode))]
public class ShapeboxGeometryNodeEditor : NodeEditor<ShapeboxGeometryNode>
{

}
[CustomEditor(typeof(AttachPointNode))]
public class AttachPointNodeEditor : NodeEditor<AttachPointNode>
{

}
[CustomEditor(typeof(SectionNode))]
public class SectionNodeEditor : NodeEditor<SectionNode>
{

}
[CustomEditor(typeof(TurboRootNode))]
public class TurboRootNodeEditor : NodeEditor<TurboRootNode>
{

}

[CustomEditor(typeof(VanillaJsonRootNode))]
public class VanillaJsonRootNodeEditor : NodeEditor<VanillaJsonRootNode>
{

}

[CustomEditor(typeof(VanillaIconRootNode))]
public class IconRootNodeEditor : NodeEditor<VanillaIconRootNode>
{

}
[CustomEditor(typeof(AnimationControllerNode))]
public class AnimationControllerNodeEditor : NodeEditor<AnimationControllerNode>
{

}

public abstract class NodeEditor<TNodeType> : Editor where TNodeType : Node
{
	private const float MODELLING_BUTTON_X = 32f;
	private static FlanStyles.FoldoutTree Tree = new FlanStyles.FoldoutTree();

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();

		if (target is TNodeType node)
		{
			// Draw top level stuff
			GUILayout.BeginHorizontal();
			GUILayout.Label("Position Snapping:");
			Node.PosSnap = (PositionSnapSetting)GUILayout.Toolbar((int)Node.PosSnap, SnapSettings.PosSnapNames);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Rotation Snapping:");
			Node.RotSnap = (RotationSnapSetting)GUILayout.Toolbar((int)Node.RotSnap, SnapSettings.RotSnapNames);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			Node parent = node.ParentNode;
			if (parent != null)
			{
				if (GUILayout.Button(FlanStyles.NavigateBack, GUILayout.Width(MODELLING_BUTTON_X)))
					Selection.activeObject = parent;
				GUILayout.Label($"Parent: {parent}");
			}

			// The root of this inspector should always start expanded
			Tree.ForceExpand(node.name);
			GUILayout.EndHorizontal();



			FlanStyles.BigHeader($"{node.name} ({typeof(TNodeType)})");

			// If we are at the root, verify this asset
			if (parent == null)
			{
				GUIVerify.CachedVerificationsBox(node);
			}

			// Quick toolbar
			GUILayout.BeginHorizontal();
			// Tools for expanding and collapsing children quickly
			if (node.transform.childCount > 0)
			{
				GUILayout.Label("Node Editor");
				if (GUILayout.Button(FlanStyles.ExpandAllNodes))
				{
					ForceExpand(node, node.name);
				}
				if (GUILayout.Button(FlanStyles.CollapseAllNodes))
				{
					Tree.ForceCollapse();
				}
			}
			GUILayout.FlexibleSpace();
			// And tools for export or finding the content pack
			if (parent == null)
			{
				GUILayout.Label("Export");
				bool validLocation = node.TryGetLocation(out ResourceLocation resLoc) && resLoc.IsContentPack();
				EditorGUI.BeginDisabledGroup(!validLocation);
				if (GUILayout.Button(validLocation ? FlanStyles.NavigateToContentPack : FlanStyles.NotInAnyContentPack))
				{
					
				}
				if (GUILayout.Button(FlanStyles.ExportSingleAsset))
				{
					
				}
				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();

		

			// Then iterate
			DrawNodeGUI(node, node.name);

			
		}

		if(EditorGUI.EndChangeCheck() && !EditorUtility.IsDirty(target))
		{
			EditorUtility.SetDirty(target);
		}
	}

	private void ForceExpand(Node node, string path)
	{
		Tree.ForceExpand(path);
		foreach (Node child in node.ChildNodes)
			ForceExpand(child, $"{path}/{child.name}");		
	}

	private void DrawNodeGUI(Node node, string path)
	{
		// -- Foldout header and quick-actions --
		GUILayout.BeginHorizontal();

		bool foldout = Tree.Foldout(FlanStyles.IconForNode(node), path);
		if(node.transform.childCount > 0)
			GUILayout.Label($"[{node.transform.childCount}]");

		if (node.SupportsRename())
		{
			string prefix = node.GetFixedPrefix();
			if (prefix.Length > 0)
			{
				string strippedName = node.name.Substring(prefix.Length);
				GUILayout.Label(prefix);
				string changedName = EditorGUILayout.DelayedTextField(strippedName);
				if (changedName != strippedName)
					node.Rename($"{prefix}{changedName}");
			}
			else
			{
				string changedName = EditorGUILayout.DelayedTextField(node.name);
				if (changedName != node.name)
					node.Rename(changedName);
			}
		}
		else
			GUILayout.Label(node.name);
		GUILayout.FlexibleSpace();

		GUIVerify.CachedVerificationsIcon(node);

		if (GUILayout.Button(FlanStyles.GoToEntry, GUILayout.Width(MODELLING_BUTTON_X)))
			Selection.activeObject = node;

		if (node.SupportsDuplicate())
			if (GUILayout.Button(FlanStyles.DuplicateEntry, GUILayout.Width(MODELLING_BUTTON_X)))
				node.Duplicate();

		if (node.SupportsDelete())
			if (GUILayout.Button(FlanStyles.DeleteEntry, GUILayout.Width(MODELLING_BUTTON_X)))
				node.Delete();

		//GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 16));
		GUILayout.EndHorizontal();

		if (foldout)
		{
			if (node.HasCompactEditorGUI() || node.SupportsTranslate() || node.SupportsRotate())
			{
				GUILayout.BeginHorizontal();
				GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 16), GUILayout.ExpandHeight(true));
				GUILayout.BeginVertical();
				FlanStyles.ThinLine();
				// Compact readout of Unity transform
				if (node.SupportsTranslate())
				{
					Vector3 newOrigin = FlanStyles.CompactVector3Field("Position", node.transform.localPosition);
					if (!newOrigin.Approximately(node.transform.localPosition))
					{
						node.Translate(newOrigin - node.transform.localPosition);
					}
				}
				if (node.SupportsRotate())
				{
					Vector3 newEuler = FlanStyles.CompactVector3Field("Euler Angles", node.transform.localEulerAngles);
					if (!newEuler.Approximately(node.transform.localEulerAngles))
					{
						node.Rotate(newEuler - node.transform.localEulerAngles);
					}
				}
				node.CompactEditorGUI();
				FlanStyles.ThinLine();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			

			foreach (Node child in node.ChildNodes)
			{
				

				EditorGUI.indentLevel++;
				if (child.HideInHeirarchy())
				{
					foreach (Node grandchild in child.ChildNodes)
					{
						DrawNodeGUI(grandchild, $"{path}/{child.name}/{grandchild.name}");
					}
				}
				else
					DrawNodeGUI(child, $"{path}/{child.name}");
				EditorGUI.indentLevel--;
			}
		}
	}
}
