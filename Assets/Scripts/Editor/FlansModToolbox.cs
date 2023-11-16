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
			string[] guids = AssetDatabase.FindAssets("t:GunDefinition");
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (path != null)
				{
					GunDefinition gun = AssetDatabase.LoadAssetAtPath<GunDefinition>(path);

					List<HandlerDefinition> handlers = new List<HandlerDefinition>(gun.inputHandlers);
					handlers.Add(new HandlerDefinition()
					{
						inputType = EPlayerInput.Reload1,
						nodes = new HandlerNodeDefinition[]{
							new HandlerNodeDefinition()
							{
								actionGroupToTrigger = "reload_primary_start",
								attachmentType = EAttachmentType.Barrel,
								attachmentIndex = 0,
							},
							new HandlerNodeDefinition()
							{
								actionGroupToTrigger = "reload_primary_start",
							}
						}
					});
					foreach(HandlerDefinition handler in handlers)
					{
						if (handler.inputType == EPlayerInput.Reload3)
							handler.inputType = EPlayerInput.SpecialKey1;
					}
					gun.inputHandlers = handlers.ToArray();

					//for (int i = 0; i < gun.inputHandlers.Length; i++)
					//{
					//	if(gun.inputHandlers[i].inputType == EPlayerInput.Fire1)
					//	{
					//		List<HandlerNodeDefinition> newNodes = new List<HandlerNodeDefinition>(gun.inputHandlers[i].nodes);
					//		newNodes.Add(new HandlerNodeDefinition()
					//		{
					//			actionGroupToTrigger = "primary_reload_start",
					//		});
					//		gun.inputHandlers[i].nodes = newNodes.ToArray();
					//	}
					//}

					//foreach(HandlerDefinition handler in gun.inputHandlers)
					//{
					//	foreach (HandlerNodeDefinition node in handler.nodes)
					//	{
					//		
					//	}
					//}

					//foreach(HandlerDefinition handler in gun.inputHandlers)
					//{
					//	foreach(HandlerNodeDefinition node in handler.nodes)
					//	{
					//		if (node.actionGroupToTrigger == "primary_reload_start")
					//			node.actionGroupToTrigger = "reload_primary_start";
					//	}
					//}
					//
					//gun.reloads = new ReloadDefinition[] {
					//	new ReloadDefinition()
					//	{
					//		startActionKey = "reload_primary_start",
					//		ejectActionKey = "reload_primary_eject",
					//		loadOneActionKey = "reload_primary_load_one",
					//		endActionKey = "reload_primary_end",
					//	}
					//};


					List<ActionGroupDefinition> groups = new List<ActionGroupDefinition>();

					//gun.primary.key = "primary";
					//groups.Add(gun.primary);
					//
					//gun.secondary.key = "ads";
					//groups.Add(gun.secondary);
					//
					//gun.lookAt.key = "look";
					//groups.Add(gun.lookAt);
					//
					//if(gun.primaryReload.start.actions.Length > 0)
					//{
					//	gun.primaryReload.start.key = "reload_primary_start";
					//	groups.Add(gun.primaryReload.start);
					//}
					//if (gun.primaryReload.eject.actions.Length > 0)
					//{
					//	gun.primaryReload.eject.key = "reload_primary_eject";
					//	groups.Add(gun.primaryReload.eject);
					//}
					//if (gun.primaryReload.loadOne.actions.Length > 0)
					//{
					//	gun.primaryReload.loadOne.key = "reload_primary_load_one";
					//	groups.Add(gun.primaryReload.loadOne);
					//}
					//if (gun.primaryReload.end.actions.Length > 0)
					//{
					//	gun.primaryReload.end.key = "reload_primary_end";
					//	groups.Add(gun.primaryReload.end);
					//}
					//gun.actionGroups = groups.ToArray();
					//
					//List<HandlerDefinition> handlers = new List<HandlerDefinition>();
					//
					//handlers.Add(new HandlerDefinition()
					//{
					//	inputType = EPlayerInput.Fire1,
					//	nodes = new HandlerNodeDefinition[] {
					//		new HandlerNodeDefinition()
					//		{
					//			actionGroupToTrigger = "primary",
					//		}
					//	}
					//});
					//
					//handlers.Add(new HandlerDefinition()
					//{
					//	inputType = EPlayerInput.Fire2,
					//	nodes = new HandlerNodeDefinition[] {
					//		new HandlerNodeDefinition()
					//		{
					//			deferToAttachment = true,
					//			attachmentType = EAttachmentType.Sights,
					//			attachmentIndex = 0,
					//		},
					//		new HandlerNodeDefinition()
					//		{
					//			deferToAttachment = true,
					//			attachmentType = EAttachmentType.Grip,
					//			attachmentIndex = 0,
					//		},
					//		new HandlerNodeDefinition()
					//		{
					//			actionGroupToTrigger = "ads",
					//		}
					//	}
					//});
					//
					//handlers.Add(new HandlerDefinition()
					//{
					//	inputType = EPlayerInput.SpecialKey1,
					//	nodes = new HandlerNodeDefinition[] {
					//		new HandlerNodeDefinition()
					//		{
					//			actionGroupToTrigger = "look"
					//		}
					//	}
					//});
					//
					//gun.inputHandlers = handlers.ToArray();
					//
					//gun.primaryMagazines.key = "primary";
					//gun.magazines = new MagazineSlotSettingsDefinition[] {
					//	gun.primaryMagazines
					//};

					EditorUtility.SetDirty(gun);
				}
			}
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
			ContentManagerEditor.ExportTab(inst);
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
	private List<ModelEditingRig> ActiveRigs = new List<ModelEditingRig>();
	private ModelEditingRig SelectedRig { get { return 0 <= SelectedRigIndex && SelectedRigIndex < ActiveRigs.Count ? ActiveRigs[SelectedRigIndex] : null; } }
	private int SelectedRigIndex = 0;
	private Editor SelectedRigEditor = null;
	private void RigsTab()
	{
		List<string> modelNames = new List<string>();
		ActiveRigs.Clear();
		foreach (ModelEditingRig rig in FindObjectsOfType<ModelEditingRig>())
		{
			ActiveRigs.Add(rig);
			modelNames.Add(rig.ModelOpenedForEdit != null ? rig.ModelOpenedForEdit.name : "No model opened");
		}

		FlanStyles.BigHeader("Rig Editor");

		if (GUILayout.Button("Create New Rig"))
		{
			GameObject newGO = new GameObject("ModelRig");
			newGO.AddComponent<ModelEditingRig>();
		}

		var RIG_COL_X = GUILayout.Width(64);
		var MODEL_COL_X = GUILayout.Width(128);
		var ATTACH_COL_X = GUILayout.Width(128);
		var AP_COL_X = GUILayout.Width(64);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Rig", RIG_COL_X);
		GUILayout.Label("Model", MODEL_COL_X);
		GUILayout.Label("Attachment", ATTACH_COL_X);
		GUILayout.Label("AP", AP_COL_X);
		GUILayout.Label("Select");
		GUILayout.EndHorizontal();

		for (int i = 0; i < ActiveRigs.Count; i++)
		{
			ModelEditingRig rig = ActiveRigs[i];
			GUILayout.BeginHorizontal();
			//if(GUILayout.Button("Inspect", GUILayout.Width(32)))
			//{
			//	SelectedRigIndex = i;
			//	Selection.SetActiveObjectWithContext(rig, this);
			//}
			EditorGUILayout.ObjectField(rig, typeof(ModelEditingRig), false, RIG_COL_X);
			ModelButton(rig, MODEL_COL_X);

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

		
		EditorGUI.BeginDisabledGroup(SelectedRig == null);
		if(SelectedRig != null)
			FlanStyles.BigHeader($"{SelectedRig.name} [{SelectedRig.ModelName}]");
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

	private void ModelButton(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Object changedModel = EditorGUILayout.ObjectField(rig.ModelOpenedForEdit, typeof(MinecraftModel), false, options);
		if (changedModel != rig.ModelOpenedForEdit)
			rig.OpenModel(changedModel as MinecraftModel);
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

	private void AttachPoseDropdown(ModelEditingRig rig, params GUILayoutOption[] options)
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
			ModelEditingRig attachToRig = ActiveRigs[index];
			if (attachToRig != rig)
			{
				APs.Add($"{attachToRig.name}_{index}");
				if (rig.transform.parent != null && rig.transform.parent.GetComponentInParent<ModelEditingRig>() == attachToRig)
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

	private void AttachPointDropdown(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Transform parent = rig.transform.parent;
		ModelEditingRig parentRig = parent?.GetComponentInParent<ModelEditingRig>();
		if (parentRig != null && parentRig.ModelOpenedForEdit is TurboRig turbo)
		{
			List<string> apNames = new List<string>();
			int attachedTo = 0;
			apNames.Add("none");
			for (int i = 0; i < turbo.AttachPoints.Count; i++)
			{
				AttachPoint ap = turbo.AttachPoints[i];
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
					string apTransformName = $"AP_{turbo.AttachPoints[changedAttachedTo - 1].name}";
					newParent = parentRig.transform.FindRecursive(apTransformName);
					if(newParent == null)
					{
						Debug.LogError($"Could not find AP '{apTransformName}' on {parentRig}");
					}
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
