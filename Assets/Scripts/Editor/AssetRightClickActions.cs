using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Animations;

public class AssetRightClickActions
{


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
}
