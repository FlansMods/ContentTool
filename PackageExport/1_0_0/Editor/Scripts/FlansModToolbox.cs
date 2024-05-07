using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class FlansModToolbox : EditorWindow
{
    [MenuItem ("Flan's Mod/Toolbox")]
    public static void ShowWindow () 
	{
        GetWindow(typeof(FlansModToolbox));
    }

	[MenuItem("Flan's Mod/Export .unitypackage")]
	public static void ExportUnityPackage()
	{
		string manifestJson = File.ReadAllText("Assets/FlansContentTool/package.json");
		string version = "0.0.0";
		using (StringReader stringReader = new StringReader(manifestJson))
		using (JsonReader jsonReader = new JsonTextReader(stringReader))
		{
			JObject jRoot = JObject.Load(jsonReader);
			if(jRoot.TryGetValue("version",out JToken jVersion) && jVersion.Type == JTokenType.String)
			{
				version = jVersion.ToString();
			}
		}
		string srcDir = "Assets/FlansContentTool/";
		string tempDir = $"PackageExport/{version.Replace('.', '_')}/";
		if (!Directory.Exists(tempDir))
			Directory.CreateDirectory(tempDir);

		foreach(string filePath in Directory.EnumerateFiles(srcDir, "*", SearchOption.AllDirectories))
		{
			try
			{
				string relativePath = filePath.Substring(filePath.IndexOf("FlansContentTool") + "FlansContentTool/".Length);

				int lastSlash = relativePath.LastIndexOfAny(Utils.SLASHES);
				if (lastSlash >= 0)
				{
					string relativeFolder = relativePath.Substring(0, lastSlash);
					string tempFolder = $"{tempDir}/{relativeFolder}";
					if (!Directory.Exists(tempFolder))
						Directory.CreateDirectory(tempFolder);
				}

				File.Copy(filePath, $"{tempDir}/{relativePath}", true);
			}
			catch(Exception e)
			{
				Debug.LogError($"Could not export {filePath} to package tmp dir {tempDir} because of {e}");
			}
		}

		UnityEditor.PackageManager.Client.Pack(tempDir, "PackageExport");


		// These packages are legacy
		//List<string> exportedAssetPaths = new List<string>();
		//exportedAssetPaths.Add("Assets/Content Packs/flansmod");
		//exportedAssetPaths.Add("Assets/Content Packs/forge");
		//exportedAssetPaths.Add("Assets/FlansContentTool/");		
		//AssetDatabase.ExportPackage(exportedAssetPaths.ToArray(), "flans_content_tool.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
	}

	public void OnEnable()
	{
		EditorApplication.update += Repaint;
	}

	public void OnDisable()
	{
		EditorApplication.update -= Repaint;
	}
	private List<ContentPack> Packs { get { return ContentManager.inst.Packs; } }
	private enum Tab
	{
		Import,
		ContentPacks,
		Rigs,
		Export,
	}
	private static readonly string[] TabNames = new string[]
	{
		"Import",
		"Content Packs",
		"Rig Editor",
		"Export",
	};

	private Tab SelectedTab = Tab.ContentPacks;
	private Vector2 scroller = Vector2.zero;
	void OnGUI()
	{
		scroller = GUILayout.BeginScrollView(scroller);
		GUILayout.BeginVertical();
		SelectedTab = (Tab)GUILayout.Toolbar((int)SelectedTab, TabNames, GUILayout.MaxWidth(Screen.width));
		switch (SelectedTab)
		{
			case Tab.Import:
				ImportTab();
				break;
			case Tab.ContentPacks:
				ContentPacksTab();
				break;
			case Tab.Rigs:
				RigsTab();
				break;
			case Tab.Export:
				ExportTab();
				break;
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}

	// -------------------------------------------------------------------------------------------------------
	#region Import Tab
	// -------------------------------------------------------------------------------------------------------
	private ContentManagerEditor ContentManagerEditor = null;
	private void CreateContentManagerEditor()
	{
		ContentManager inst = ContentManager.inst;
		if (ContentManagerEditor == null || ContentManagerEditor.target != inst)
		{
			ContentManagerEditor = Editor.CreateEditor(inst) as ContentManagerEditor;
		}
	}

	private void ImportTab()
	{
		CreateContentManagerEditor();
		ContentManager inst = ContentManager.inst;
		if (ContentManagerEditor != null)
			ContentManagerEditor.ImportTab(inst);
	}
	#endregion
	// -------------------------------------------------------------------------------------------------------

	// -------------------------------------------------------------------------------------------------------
	#region Export Tab
	// -------------------------------------------------------------------------------------------------------
	private void ExportTab()
	{
		CreateContentManagerEditor();
		ContentManager inst = ContentManager.inst;
		if (ContentManagerEditor != null)
			ContentManagerEditor.ExportTab(inst, (pack) => {
				SelectedTab = Tab.ContentPacks;
				SelectedContentPackIndex = Packs.IndexOf(pack);
			});
	}
	#endregion
	// -------------------------------------------------------------------------------------------------------

	// -------------------------------------------------------------------------------------------------------
	#region Content Packs Tab
	// -------------------------------------------------------------------------------------------------------
	private int SelectedContentPackIndex = -1;
	private string SelectedContentPackName { get { return SelectedContentPackIndex >= 0 ? Packs[SelectedContentPackIndex].ModName : "None"; } }
	private ContentPack SelectedContentPack { get { return SelectedContentPackIndex >= 0 ? Packs[SelectedContentPackIndex] : null; } }
	private Editor ContentPackEditor = null;
	private void ContentPacksTab()
	{
		FlanStyles.BigHeader("Content Packs");
		List<string> packNames = new List<string>();
		packNames.Add("None");
		for (int i = 0; i < Packs.Count; i++)
		{
			ContentPack pack = Packs[i];
			packNames.Add(pack.ModName);
		}
		SelectedContentPackIndex = EditorGUILayout.Popup(SelectedContentPackIndex+1, packNames.ToArray()) - 1;
		EditorGUILayout.ObjectField(SelectedContentPack, typeof(ContentPack), false);

		if(ContentPackEditor == null || ContentPackEditor.target != SelectedContentPack)
		{
			ContentPackEditor = Editor.CreateEditor(SelectedContentPack);
		}
		if(ContentPackEditor != null)
		{
			ContentPackEditor.OnInspectorGUI();
		}
	}
	#endregion
	// -------------------------------------------------------------------------------------------------------

	// -------------------------------------------------------------------------------------------------------
	#region Rigs Tab
	// -------------------------------------------------------------------------------------------------------
	private List<RootNode> ActiveRigs = new List<RootNode>();
	private RootNode SelectedRig { get { return 0 <= SelectedRigIndex && SelectedRigIndex < ActiveRigs.Count ? ActiveRigs[SelectedRigIndex] : null; } }
	private int SelectedRigIndex = 0;
	private Editor SelectedRigEditor = null;
	private FlanStyles.FoldoutTree RigsTree = new FlanStyles.FoldoutTree();
	private void RigsTab()
	{
		bool forceUpdate = false;
		List<string> modelNames = new List<string>();
		if (ActiveRigs.Count == 0 || forceUpdate)
		{
			ActiveRigs.Clear();
			foreach (RootNode rig in FindObjectsOfType<RootNode>())
			{
				ActiveRigs.Add(rig);
				modelNames.Add(rig.name);
			}
			ActiveRigs.Sort((RootNode a, RootNode b) =>
			{
				return string.Compare(a.name, b.name);
			});
		}

		FlanStyles.BigHeader("Rig Editor");

		if(GUILayout.Button("Fan out all Rigs"))
		{
			GameObject root = GameObject.Find("Rig Gallery");
			if (root == null)
				root = new GameObject("Rig Gallery");

			for(int i = 0; i < ActiveRigs.Count; i++)
			{
				RootNode rig = ActiveRigs[i];
				rig.transform.SetParent(root.transform);
				rig.transform.localPosition = new Vector3(i * 10f, 0f, 0f);
				rig.transform.localRotation = Quaternion.identity;
				rig.transform.localScale = Vector3.one;
			}
		}

		var RIG_COL_X = GUILayout.Width(64);
		var MODEL_COL_X = GUILayout.Width(128);
		var ATTACH_COL_X = GUILayout.Width(128);
		var AP_COL_X = GUILayout.Width(64);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Rig", RIG_COL_X);
		GUILayout.Label("Attachment", ATTACH_COL_X);
		GUILayout.Label("AP", AP_COL_X);
		GUILayout.Label("Select");
		GUILayout.EndHorizontal();

		for (int i = 0; i < ActiveRigs.Count; i++)
		{
			RootNode rig = ActiveRigs[i];
			bool foldout = RigsTree.Foldout(new GUIContent(rig.name), rig.name);
			if (foldout)
			{
				GUILayout.BeginHorizontal();
				//if(GUILayout.Button("Inspect", GUILayout.Width(32)))
				//{
				//	SelectedRigIndex = i;
				//	Selection.SetActiveObjectWithContext(rig, this);
				//}
				EditorGUILayout.ObjectField(rig, typeof(RootNode), false, RIG_COL_X);
				AttachPoseDropdown(rig, ATTACH_COL_X);
				AttachPointDropdown(rig, AP_COL_X);

				EditorGUI.BeginDisabledGroup(SelectedRigIndex == i);
				if (GUILayout.Button("Select"))
				{
					SelectedRigIndex = i;
					Selection.SetActiveObjectWithContext(SelectedRig, this);
				}
				EditorGUI.EndDisabledGroup();

				GUILayout.EndHorizontal();
			}
		}

		
		EditorGUI.BeginDisabledGroup(SelectedRig == null);
		if(SelectedRig != null)
			FlanStyles.BigHeader($"{SelectedRig.name}");
		if(SelectedRigEditor == null || SelectedRigEditor.target != SelectedRig)
		{
			SelectedRigEditor = Editor.CreateEditor(SelectedRig);
		}
		if(SelectedRigEditor != null)
		{
			SelectedRigEditor.OnInspectorGUI();
		}		
		EditorGUI.EndDisabledGroup();
	}

	private static readonly string[] APDefaults = new string[] {
		"NotAttached",
		"DefaultPose",
		"Alex_RightHandPose",
		"Alex_LeftHandPose",
		"Steve_RightHandPose",
		"Steve_LeftHandPose",
		"FirstPerson_RightHandPose",
		"FirstPerson_LeftHandPose",
		"GUIPose",
		"GreenScreenPose",
	};

	private void AttachPoseDropdown(RootNode rig, params GUILayoutOption[] options)
	{
		List<string> APs = new List<string>(APDefaults);

		// First, check if this is attached to one of our known parents
		int selectedIndex = 0;
		if (rig.transform.parent != null)
		{
			selectedIndex = APs.IndexOf(rig.transform.parent.name);
			if (selectedIndex == -1)
				selectedIndex = 0;
		}

		// Then check if this is attached to another rig
		int myRigIndex = 0;
		for (int index = 0; index < ActiveRigs.Count; index++)
		{
			RootNode attachToRig = ActiveRigs[index];
			if (attachToRig != rig)
			{
				APs.Add($"{attachToRig.name}_{index}");
				if (rig.transform.parent != null && rig.transform.parent.GetComponentInParent<RootNode>() == attachToRig)
					selectedIndex = APs.Count - 1;
			}
			else
				myRigIndex = index;
		}

		int changedIndex = EditorGUILayout.Popup(selectedIndex, APs.ToArray(), options);
		if (changedIndex != selectedIndex)
		{
			Transform attachTo = null;
			if (changedIndex >= APDefaults.Length)
			{
				int relativeIndex = changedIndex - APDefaults.Length;
				if (relativeIndex >= myRigIndex)
					relativeIndex++;
				attachTo = ActiveRigs[relativeIndex].transform;
			}
			else
				attachTo = GameObject.Find(APs[changedIndex])?.transform;
			rig.transform.SetParent(attachTo);
			rig.transform.localPosition = Vector3.zero;
			rig.transform.localRotation = Quaternion.identity;
			rig.transform.localScale = Vector3.one;
		}
	}

	private void AttachPointDropdown(RootNode rig, params GUILayoutOption[] options)
	{
		Transform parent = rig.transform.parent;
		RootNode parentRig = parent?.GetComponentInParent<RootNode>();
		if (parentRig != null)
		{
			List<string> apNames = new List<string>();
			List<AttachPointNode> apNodes = new List<AttachPointNode>(parentRig.GetAllDescendantNodes<AttachPointNode>());
			int attachedTo = 0;
			apNames.Add("none");
			for (int i = 0; i < apNodes.Count; i++)
			{
				AttachPointNode ap = apNodes[i];
				apNames.Add(ap.name);
				if (ap.name == parent.name)
					attachedTo = i + 1;
			}
			int changedAttachedTo = EditorGUILayout.Popup(attachedTo, apNames.ToArray(), options);
			if (changedAttachedTo != attachedTo)
			{
				Transform newParent = parentRig.transform;
				if (changedAttachedTo != 0)
				{
					newParent = apNodes[changedAttachedTo - 1].transform;
				}
				rig.transform.SetParent(newParent);
				rig.transform.localPosition = Vector3.zero;
				rig.transform.localRotation = Quaternion.identity;
				rig.transform.localScale = Vector3.one;
			}
		}
		else
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Popup(0, new string[] { "N/A" }, options);
			EditorGUI.EndDisabledGroup();
		}
	}

	#endregion
	// -------------------------------------------------------------------------------------------------------
}
