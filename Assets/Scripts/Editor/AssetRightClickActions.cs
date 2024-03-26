using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Design;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AssetRightClickActions
{

	[MenuItem("Assets/Flan's Mod/FlanimationDef -> AnimatorController")]
	private static bool ConvertToUnityAnimsApply()
	{
		foreach (string selectedGUID in Selection.assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
			System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			FlanimationDefinition flanimation = AssetDatabase.LoadAssetAtPath<FlanimationDefinition>(assetPath);
			if (flanimation != null)
			{
				UnityAnimationExporter.INST.ConvertToUnityAnim(flanimation);
			}
		}
		return false;
	}
	[MenuItem("Assets/Flan's Mod/FlanimationDef -> AnimatorController", true)]
	private static bool ConvertToUnityAnimsValidation()
	{
		foreach (string selectedGUID in Selection.assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
			System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (typeof(FlanimationDefinition).IsAssignableFrom(assetType))
				return true;
		}
		return false;
	}



	[MenuItem("Assets/Flan's Mod/Export")]
	private static bool ExportApply()
	{
		foreach (string selectedGUID in Selection.assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
			System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType); 
			if (asset != null)
			{
				if (FlansModExport.TryGetExporter(asset, out AssetExporter exporter))
				{
					exporter.Export(asset);
				}
			}
		}
		return false;
	}
	[MenuItem("Assets/Flan's Mod/Export", true)]
	private static bool ExportValidation()
	{
		foreach (string selectedGUID in Selection.assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
			System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (FlansModExport.IsExportableType(assetType))
				return true;
		}
		return false;
	}

	[MenuItem("CONTEXT/RootNode/Add ItemPoseNode")]
	private static bool CreateItemPoseNode(UnityEditor.MenuCommand menuCommand) { return Create<ItemPoseNode>(menuCommand); }
	[MenuItem("CONTEXT/RootNode/Add SectionNode")]
	private static bool CreateSectionNode(UnityEditor.MenuCommand menuCommand) { return Create<SectionNode>(menuCommand); }
	[MenuItem("CONTEXT/AttachPointNode/Add SectionNode")]
	private static bool CreateSectionNode2(UnityEditor.MenuCommand menuCommand) { return Create<SectionNode>(menuCommand); }
	[MenuItem("CONTEXT/RootNode/Add AttachPointNode")]
	private static bool CreateAPNode(UnityEditor.MenuCommand menuCommand) { return Create<AttachPointNode>(menuCommand); }
	[MenuItem("CONTEXT/AttachPointNode/Add AttachPointNode")]
	private static bool CreateAPNode2(UnityEditor.MenuCommand menuCommand) { return Create<AttachPointNode>(menuCommand); }

	[MenuItem("CONTEXT/SectionNode/Add BoxGeometryNode")]
	private static bool CreateBoxNode(UnityEditor.MenuCommand menuCommand) { return Create<BoxGeometryNode>(menuCommand); }
	[MenuItem("CONTEXT/SectionNode/Add ShapeboxGeometryNode")]
	private static bool CreateShapeboxNode(UnityEditor.MenuCommand menuCommand) { return Create<ShapeboxGeometryNode>(menuCommand); }

	[MenuItem("GameObject/Create New Flan's Turbo Rig")]
	private static bool CreateTurboRootNode(UnityEditor.MenuCommand menuCommand) { return Create<TurboRootNode>(menuCommand); }

	private static bool Create<TNodeType>(UnityEditor.MenuCommand menuCommand) where TNodeType : Node
	{
		try
		{
			GameObject go = new GameObject("New");
			TNodeType turboNode = go.AddComponent<TNodeType>();
			if (turboNode is RootNode root)
				root.AddDefaultTransforms();
			go.name = $"{turboNode.GetFixedPrefix()}new";
			GameObjectUtility.SetParentAndAlign(go, (menuCommand.context as Component).gameObject);
			Undo.RegisterCreatedObjectUndo(go, $"Created {go}");
			Selection.activeObject = go;
			return true;
		}
		catch (Exception)
		{

		}
		return false;
	}
}
