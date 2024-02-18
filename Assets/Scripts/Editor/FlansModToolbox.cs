using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FlansModToolbox : EditorWindow
{
    [MenuItem ("Flan's Mod/Toolbox")]
    public static void ShowWindow () 
	{
        GetWindow(typeof(FlansModToolbox));
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

		// Temp - For Flan if I need to iterate some things in editor quickly
		if (GUILayout.Button("Do the thing"))
		{


			string[] guids = AssetDatabase.FindAssets("t:Definition");
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (path != null)
				{
					Definition def = AssetDatabase.LoadAssetAtPath<Definition>(path);
					//FlanimationDefinition anim = AssetDatabase.LoadAssetAtPath<FlanimationDefinition>(path);
					//foreach (KeyframeDefinition keyframe in anim.keyframes)
					//{
					//	foreach (PoseDefinition pose in keyframe.poses)
					//	{
					//		pose.position = new VecWithOverride()
					//		{
					//			xValue = pose.position.zValue,
					//			yValue = pose.position.yValue,
					//			zValue = -pose.position.xValue,
					//			xOverride = pose.position.xOverride,
					//			yOverride = pose.position.yOverride,
					//			zOverride = pose.position.zOverride
					//		};
					//		pose.rotation = new VecWithOverride()
					//		{
					//			xValue = -pose.rotation.zValue,
					//			yValue = pose.rotation.yValue,
					//			zValue = pose.rotation.xValue,
					//			xOverride = pose.rotation.xOverride,
					//			yOverride = pose.rotation.yOverride,
					//			zOverride = pose.rotation.zOverride
					//		};
					//	}
					//}
					EditorUtility.SetDirty(def);
				}
			}
				

			/*
				string[] guids = AssetDatabase.FindAssets("t:TurboRig");
				foreach (string guid in guids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					if (path != null)
					{
						TurboRig rig = AssetDatabase.LoadAssetAtPath<TurboRig>(path);
						foreach (ItemTransform transform in rig.Transforms)
						{
							if(transform.Type == ItemTransformType.FIRST_PERSON_LEFT_HAND ||
								transform.Type == ItemTransformType.FIRST_PERSON_RIGHT_HAND)
							{
								transform.Rotation = Quaternion.Euler(0f, -90f, 0f);
							}
						}
						foreach(AttachPoint ap in rig.AttachPoints)
						{
							if(ap.name == "laser_origin" || ap.name == "shoot_origin" || ap.name == "eye_line"
							|| ap.name.StartsWith("barrel")
							|| ap.name.StartsWith("stock")
							|| ap.name.StartsWith("grip")
							|| ap.name.StartsWith("sights"))
							{
								if(Mathf.Approximately(ap.euler.y, 0f))
								{
									TurboModel section = rig.GetSection(ap.name);
									if(section != null)
									{
										foreach(TurboPiece piece in section.Pieces)
										{
											piece.Pos += Quaternion.Euler(piece.Euler) * piece.Origin;
											piece.Origin = Vector3.zero;
											piece.Euler += new Vector3(0f, 90f, 0f);
										}
									}
								}
								ap.euler = new Vector3(0, -90f, 0f);
							}
							else if(ap.name == "barrel" || ap.name == "grip" || ap.name == "")
							{
								//ap.euler = new Vector3(0f, 90f, 0f);
							}
						}
						EditorUtility.SetDirty(rig);
					}
				}
				*/
			/*
			guids = AssetDatabase.FindAssets("t:MagazineDefinition");
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (path != null)
				{
					MagazineDefinition mag = AssetDatabase.LoadAssetAtPath<MagazineDefinition>(path);
					foreach (ModifierDefinition modifier in mag.modifiers)
						if (modifier.ApplyFilters.Length > 0 && modifier.ApplyFilters[0] == "primary_reload")
							modifier.ApplyFilters[0] = "reload_primary";
					EditorUtility.SetDirty(mag);
				}
			}
			*/
		}
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
