using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using UnityEditor.Build;
using Unity.VisualScripting;

public class FlansModToolbox : EditorWindow
{
    [MenuItem ("Flan's Mod/Toolbox")]
    public static void  ShowWindow () 
	{
        EditorWindow.GetWindow(typeof(FlansModToolbox));
    }

	public void OnEnable()
	{
		EditorApplication.update += Repaint;
	}

	public void OnDisable()
	{
		EditorApplication.update -= Repaint;
	}

	private DefinitionImporter DefinitionImporter = null;
	private List<ContentPack> Packs 
	{
		get	
		{
			if (DefinitionImporter == null)
				DefinitionImporter = FindObjectOfType<DefinitionImporter>();
			return DefinitionImporter.Packs;
		}
	}
	private enum Tab
	{
		Import,
		ContentPacks,
		Rigs,
	}
	private static readonly string[] TabNames = new string[]
	{
		"Import",
		"Content Packs",
		"Rig Editor",
	};

	private Tab SelectedTab = Tab.ContentPacks;
	

	private string recipeFolder = "";
	private string copyFromMat = "iron";
	private string copyToMat = "aluminium";

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
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}

	// -------------------------------------------------------------------------------------------------------
	#region Import Tab
	// -------------------------------------------------------------------------------------------------------
	private enum ImportSubTab
	{
		
	}
	private int SelectedImportPackIndex = 0;
	private List<string> ImportFoldouts = new List<string>();
	private void ImportTab()
	{
		DefinitionImporter inst = DefinitionImporter.inst;
		// TODO: Timer on this
		inst.CheckInit();

		foreach (string sourcePack in inst.UnimportedPacks)
		{
			string packFoldoutPath = $"{sourcePack}";
			GUILayout.BeginHorizontal();
			bool packFoldout = NestedFoldout(packFoldoutPath, sourcePack);
			GUILayout.Label("t", GUILayout.Width(32));
			GUILayout.EndHorizontal();
			if (packFoldout)
			{
				EditorGUI.indentLevel++;
				// TODO:Sort by type
				Dictionary<string, string> importMap = CreateImportMap(sourcePack);
				// tODO: Gather import data "will this override existing etc"
				foreach(var kvp in importMap)
				{
					string importFoldoutPath = $"{sourcePack}/{kvp.Key}";
					if (NestedFoldout(importFoldoutPath, kvp.Key))
					{
						EditorGUI.indentLevel++;
						GUILayout.Label(kvp.Value);
						EditorGUI.indentLevel--;
					}
				}

				EditorGUI.indentLevel--;
			}
		}
	}

	private Dictionary<string, string> CreateImportMap(string importFolder)
	{
		Dictionary<string, string> importMap = new Dictionary<string, string>();
		string modName = Utils.ToLowerWithUnderscores(importFolder);
		for (int i = 0; i < DefinitionTypes.NUM_TYPES; i++)
		{
			EDefinitionType defType = (EDefinitionType)i;
			DirectoryInfo dir = new DirectoryInfo($"{DefinitionImporter.IMPORT_ROOT}/{importFolder}/{defType.Folder()}");
			if (dir.Exists)
			{
				foreach (FileInfo file in dir.EnumerateFiles())
				{
					string shortName = Utils.ToLowerWithUnderscores(file.Name.Split(".")[0]);
					importMap.Add(
						$"{importFolder}/{defType.Folder()}/{file.Name}",
						$"{modName}/{defType.OutputFolder()}/{shortName}.asset");
				}
			}
		}
		return importMap;
	}

	/*
	private void p()
	{


		// Back to front? Should be imports

		foreach (ContentPack pack in inst.Packs)
		{
			string packPath = $"{pack.name}";

			List<string> originalAssetPaths = new List<string>();
			bool hasOriginalImportFolder = false;
			if (inst.UnimportedPacks.Contains(pack.name))
			{
				hasOriginalImportFolder = true;
			}

			if (NestedFoldout(packPath, pack.name))
			{
				EditorGUI.indentLevel++;

				GUILayout.Label($"Pack: {pack.name}", FlanStyles.BoldLabel);
				GUILayout.Label(hasOriginalImportFolder ? $"Matching source assets at Import/Content Packs/{pack.name}" : "No matching assets in Import/Content Packs", FlanStyles.BoldLabel);

				string defsPath = $"{packPath}/Definitions";
				if(NestedFoldout(defsPath, "Definitions"))
				{
					EditorGUI.indentLevel++;
					foreach (var kvp in pack.GetSortedContent())
					{
						string typePath = $"{defsPath}/{kvp.Key}";
						if (NestedFoldout(typePath, kvp.Key.ToString()))
						{
							EditorGUI.indentLevel++;
							foreach (Definition def in kvp.Value)
							{
								ContentNode(def, pack.name);
							}
							EditorGUI.indentLevel--;
						}
					}
					
					EditorGUI.indentLevel--;
				}
				string modelsPath = $"{packPath}/Models";
				if (NestedFoldout(modelsPath, "Models"))
				{

				}
				EditorGUI.indentLevel--;
			}
			
		}


		// Import location
		FolderSelector("Export Location", inst.ExportRoot, "Assets/Export");
	}
	*/
	private void ContentNode(Definition def, string parentPath)
	{
		string path = $"{parentPath}/{def.name}";
		if(NestedFoldout(path, def.name))
		{ 
			
		}
	}
	private bool NestedFoldout(string path, string label)
	{
		bool oldFoldout = ImportFoldouts.Contains(path);
		bool newFoldout = EditorGUILayout.Foldout(oldFoldout, label);
		if (newFoldout && !oldFoldout)
			ImportFoldouts.Add(path);
		else if (oldFoldout && !newFoldout)
			ImportFoldouts.Remove(path);
		return newFoldout;
	}



	private string FolderSelector(string label, string folder, string defaultLocation)
	{
		GUILayout.Label(label);
		GUILayout.BeginHorizontal();
		folder = EditorGUILayout.DelayedTextField(folder);
		if(GUILayout.Button(EditorGUIUtility.IconContent("d_Profiler.Open")))
			folder = EditorUtility.OpenFolderPanel("Select resources root folder", folder, "");
		if (GUILayout.Button(EditorGUIUtility.IconContent("d_preAudioLoopOff")))
			folder = defaultLocation;
		GUILayout.EndHorizontal();
		return folder;
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
	private RigsSubTab SubTab = RigsSubTab.Models;
	private enum RigsSubTab 
	{
		Models,
		Animations,
		Skins,
	}
	private static readonly string[] SubTabNames = new string[] {
		"Model",
		"Animations",
		"Skin",
	};
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
		SubTab = (RigsSubTab)GUILayout.Toolbar((int)SubTab, SubTabNames);
		switch(SubTab)
		{
			case RigsSubTab.Models:
				ModelsTab();
				break;
			case RigsSubTab.Animations:
				AnimationsTab();
				break;
			case RigsSubTab.Skins:
				SkinsTab();
				break;
		}
		EditorGUI.EndDisabledGroup();
	}

	private Editor ModelSubEditor = null;
	private bool ModelEditorFoldout = false;
	private void ModelsTab()
	{
		if (SelectedRig == null)
			return;

		ModelButton(SelectedRig);
		if (SelectedRig.ModelOpenedForEdit != null)
		{
			bool dirty = EditorUtility.IsDirty(SelectedRig.ModelOpenedForEdit);
			GUILayout.Label(dirty ? $"*{SelectedRig.ModelOpenedForEdit.name} has unsaved changes" : $"{SelectedRig.ModelOpenedForEdit.name} has no changes.");
		}

		EditorGUI.BeginDisabledGroup(SelectedRig.ModelOpenedForEdit == null);
		ModelEditorFoldout = EditorGUILayout.Foldout(ModelEditorFoldout, "Model Editor");
		if (ModelEditorFoldout)
		{
			if (ModelSubEditor == null || ModelSubEditor.target != SelectedRig.ModelOpenedForEdit)
			{
				ModelSubEditor = Editor.CreateEditor(SelectedRig.ModelOpenedForEdit);
			}
			if (ModelSubEditor != null)
			{
				ModelSubEditor.OnInspectorGUI();
			}
		}
		EditorGUI.EndDisabledGroup();
	}

	private Editor AnimationSubEditor = null;
	private bool AnimationEditorFoldout = false;
	private const int PREVIEW_INDEX_COL_X = 20;
	private const int PREVIEW_DROPDOWN_COL_X = 128;
	private const int PREVIEW_DURATION_COL_X = 40;
	private const int PREVIEW_REMOVE_COL_X = 20;
	private void AnimationsTab()
	{
		if (SelectedRig == null)
			return;

		SelectedRig.ApplyAnimation = GUILayout.Toggle(SelectedRig.ApplyAnimation, "Preview Animations");

		EditorGUI.BeginDisabledGroup(!SelectedRig.ApplyAnimation);
		AnimationButton(SelectedRig);
		if (SelectedRig.SelectedAnimation != null)
		{
			bool dirty = EditorUtility.IsDirty(SelectedRig.SelectedAnimation);
			GUILayout.Label(dirty ? $"*{SelectedRig.SelectedAnimation.name} has unsaved changes" : $"{SelectedRig.SelectedAnimation.name} has no changes.");
		}

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey")))
			SelectedRig.PressBack();
		if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton")))
			SelectedRig.PressPlay();
		if (GUILayout.Button(EditorGUIUtility.IconContent("PauseButton")))
			SelectedRig.PressPause();

		SelectedRig.Looping = GUILayout.Toggle(SelectedRig.Looping, "Repeat");
		SelectedRig.StepThrough = GUILayout.Toggle(SelectedRig.StepThrough, "Step-by-Step");
		GUILayout.EndHorizontal();

		List<string> keyframeNames = new List<string>();
		List<string> sequenceNames = new List<string>();
		List<string> allNames = new List<string>();
		if (SelectedRig.SelectedAnimation != null)
		{
			foreach (KeyframeDefinition keyframeDef in SelectedRig.SelectedAnimation.keyframes)
			{
				keyframeNames.Add(keyframeDef.name);
				allNames.Add($"Keyframe:{keyframeDef.name}");
			}
			foreach (SequenceDefinition sequenceDef in SelectedRig.SelectedAnimation.sequences)
			{
				sequenceNames.Add(sequenceDef.name);
				allNames.Add($"Sequence:{sequenceDef.name}");
			}
		}

		float animProgress = SelectedRig.GetPreviewProgressSeconds();
		float animDuration = SelectedRig.GetPreviewDurationSeconds();

		GUILayout.Label($"[*] Previews ({animProgress.ToString("0.00")}/{animDuration.ToString("0.00")})");

		int previewIndex = -1;
		float animParameter = 0.0f;
		ModelEditingRig.AnimPreviewEntry currentPreview = SelectedRig.GetCurrentPreviewEntry(out previewIndex, out animParameter);
		int indexToRemove = -1;

		// ------------------------------------------------------------------------
		// For each entry, render the settings and mark it in green/bold if current
		for (int i = 0; i < SelectedRig.PreviewSequences.Count; i++)
		{
			GUILayout.BeginHorizontal();
			ModelEditingRig.AnimPreviewEntry entry = SelectedRig.PreviewSequences[i];
			float previewDurationSeconds = SelectedRig.GetDurationSecondsOf(entry);
			int previewDurationTicks = SelectedRig.GetDurationTicksOf(entry);

			float currentSeconds = animParameter * previewDurationSeconds;
			int currentTicks = Mathf.FloorToInt(currentSeconds * 20f);
			if (entry == currentPreview)
				FlanStyles.SelectedLabel($"[{i + 1}]", GUILayout.Width(PREVIEW_INDEX_COL_X));
			else
				GUILayout.Label($"[{i + 1}]", GUILayout.Width(PREVIEW_INDEX_COL_X));
			string compactName = $"{(entry.IsSequence ? "Sequence" : "Keyframe")}:{entry.Name}";
			int selectedIndex = allNames.IndexOf(compactName);
			int modifiedIndex = EditorGUILayout.Popup(selectedIndex, allNames.ToArray(), GUILayout.Width(PREVIEW_DROPDOWN_COL_X));
			if(selectedIndex != modifiedIndex)
			{
				entry.IsSequence = allNames[modifiedIndex].Contains("Sequence:");
				entry.Name = allNames[modifiedIndex];
				entry.Name = entry.Name.Substring(entry.Name.IndexOf(":") + 1);
			}

			EditorGUI.BeginDisabledGroup(entry.IsSequence);
			int durationTicks = SelectedRig.GetDurationTicksOf(entry);
			entry.DurationTicks = EditorGUILayout.IntField(durationTicks, GUILayout.Width(PREVIEW_DURATION_COL_X));
			EditorGUI.EndDisabledGroup();

			if (entry == currentPreview)
				FlanStyles.SelectedLabel($"({currentSeconds.ToString("0.00")} / {previewDurationSeconds.ToString("0.00")}) | ({currentTicks}t / {previewDurationTicks}t)");
			else
				GUILayout.Label($"({previewDurationSeconds.ToString("0.00")}) | ({previewDurationTicks}t)");

			if (GUILayout.Button("-", GUILayout.Width(PREVIEW_REMOVE_COL_X)))
				indexToRemove = i;

			GUILayout.EndHorizontal();
		}

		if (indexToRemove != -1)
			SelectedRig.PreviewSequences.RemoveAt(indexToRemove);

		// ----------------------------------------------------------------------
		// Extra element that gets "selected" if you are at the end, also an add button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("+", GUILayout.Width(PREVIEW_INDEX_COL_X)))
			SelectedRig.PreviewSequences.Add(new ModelEditingRig.AnimPreviewEntry()
			{
				Name = "",
				DurationTicks = 20,
				IsSequence = false,
			});
		if (currentPreview == null)
		{
			FlanStyles.SelectedLabel("Finished", GUILayout.Width(PREVIEW_DROPDOWN_COL_X));
			FlanStyles.SelectedLabel("-", GUILayout.Width(PREVIEW_DURATION_COL_X));
			FlanStyles.SelectedLabel("");
		}
		else
		{
			GUILayout.Label("Finished", GUILayout.Width(PREVIEW_DROPDOWN_COL_X));
			GUILayout.Label("-", GUILayout.Width(PREVIEW_DURATION_COL_X));
			GUILayout.Label("");
		}
		GUILayout.EndHorizontal();


		// ----------------------------------------------------------------------
		// Timeline slider
		GUILayout.BeginHorizontal();
		for (int i = 0; i < SelectedRig.PreviewSequences.Count; i++)
		{
			float previewSeconds = SelectedRig.GetDurationSecondsOf(i);
			float guiWidth = Screen.width * previewSeconds / animDuration;
			if(GUILayout.Button(SelectedRig.PreviewSequences[i].Name, FlanStyles.BorderlessButton, GUILayout.Width(guiWidth)))
			{
				SelectedRig.SetPreviewIndex(i);
			}
		}
		GUILayout.EndHorizontal();
		float edited = GUILayout.HorizontalSlider(animProgress, 0f, animDuration);
		if (!Mathf.Approximately(edited, animProgress))
			SelectedRig.SetPreviewProgressSeconds(edited);
		GUILayout.Space(16);
		// ----------------------------------------------------------------------

		AnimationEditorFoldout = EditorGUILayout.Foldout(AnimationEditorFoldout, "Animation Editor");
		if(AnimationEditorFoldout)
		{
			if(AnimationSubEditor == null || AnimationSubEditor.target != SelectedRig.SelectedAnimation)
			{
				AnimationSubEditor = Editor.CreateEditor(SelectedRig.SelectedAnimation);
			}
			if (AnimationSubEditor != null)
			{
				AnimationSubEditor.OnInspectorGUI();
			}
		}
		EditorGUI.EndDisabledGroup();
	}

	private static readonly List<float> TextureZoomSettings = new List<float>(new float[] { 0, 1, 2, 4, 8, 16, 32, 64 });
	private static readonly string[] TextureZoomSettingNames = new string[] { "Auto", "1", "2", "4", "8", "16", "32", "64" };
	private List<int> ExpandedTextures = new List<int>();
	private void RenderTextureAutoWidth(Texture texture)
	{
		if (MinecraftModelPreview.TextureZoomLevel == 0)
		{
			float scale = (float)(Screen.width - 10) / texture.width;
			GUILayout.Label(GUIContent.none,
							GUILayout.Width(texture.width * scale),
							GUILayout.Height(texture.height * scale));
		}
		else
		{
			GUILayout.Label(GUIContent.none,
							GUILayout.Width(texture.width * MinecraftModelPreview.TextureZoomLevel),
							GUILayout.Height(texture.height * MinecraftModelPreview.TextureZoomLevel));
		}
		GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
	}
	private void SkinsTab()
	{
		if (SelectedRig == null)
			return;

		GUILayout.Label("Texture Zoom Level", FlanStyles.BoldLabel);
		int oldIndex = TextureZoomSettings.IndexOf(MinecraftModelPreview.TextureZoomLevel);
		int newIndex = GUILayout.Toolbar(oldIndex, TextureZoomSettingNames);
		MinecraftModelPreview.TextureZoomLevel = TextureZoomSettings[newIndex];

		FlanStyles.BigHeader("Skins");
		GUILayout.Label(SelectedRig.SelectedSkin);
		if(SelectedRig.Preview is TurboRigPreview turboPreview)
		{
			// Draw a box for each texture
			for(int i = 0; i < turboPreview.Rig.Textures.Count; i++)
			{
				MinecraftModel.NamedTexture texture = turboPreview.Rig.Textures[i];
				List<Verification> verifications = new List<Verification>();
				texture.GetVerifications(verifications);
				bool oldExpanded = ExpandedTextures.Contains(i);

				GUILayout.BeginHorizontal();
				bool newExpanded = EditorGUILayout.Foldout(oldExpanded, GUIContent.none);
				if (oldExpanded && !newExpanded)
					ExpandedTextures.Remove(i);
				else if (newExpanded && !oldExpanded)
					ExpandedTextures.Add(i);
				GUILayout.Label($"[{i}]", GUILayout.Width(32));
				GUIVerify.VerificationIcon(verifications);

				texture.Location = ResourceLocation.EditorObjectField<Texture2D>(texture.Location, "textures/skins");
				GUILayout.EndHorizontal();

				if(newExpanded)
				{
					if (texture.Texture != null)
					{
						RenderTextureAutoWidth(texture.Texture);
					}
				}
			}

			// And a row for "add new"
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("[+]", GUILayout.Width(32)))
			{
				turboPreview.Rig.Textures.Add(new MinecraftModel.NamedTexture()
				{
					Key = "new_skin",
					Location = new ResourceLocation(turboPreview.Rig.GetLocation().Namespace, "null"),
					Texture = null,
				});
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}



		FlanStyles.BigHeader("UV Mapping");

		GUILayout.BeginHorizontal();
		

		//MinecraftModelPreview.TextureZoomLevel = Mathf.Clamp(EditorGUILayout.FloatField(MinecraftModelPreview.TextureZoomLevel), 1f, 256f);
		//MinecraftModelPreview.TextureZoomLevel = EditorGUILayout.Slider(MinecraftModelPreview.TextureZoomLevel, 1f, 256f);
		GUILayout.EndHorizontal();
		InitialSkinNode(SelectedRig.Preview);
	}
	private List<string> SkinNodeFoldouts = new List<string>();
	private void InitialSkinNode(MinecraftModelPreview preview)
	{
		SkinNode(preview, "");
	}
	private void SkinNode(MinecraftModelPreview preview, string path)
	{
		if (preview == null || EditorGUI.indentLevel > 16) 
			return;
		string childPath = $"{path}/{preview.name}";
		bool foldout = SkinNodeFoldouts.Contains(childPath);
		bool newFoldout = EditorGUILayout.Foldout(foldout, preview.name);
		if (newFoldout && !foldout)
			SkinNodeFoldouts.Add(childPath);
		else if (!newFoldout && foldout)
			SkinNodeFoldouts.Remove(childPath);

		if (newFoldout)
		{
			preview.Compact_Editor_Texture_GUI();
			EditorGUI.indentLevel++;
			foreach (MinecraftModelPreview child in preview.GetChildren())
			{
				SkinNode(child, childPath);
			}
			EditorGUI.indentLevel--;
		}
	}

	private void ModelButton(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Object changedModel = EditorGUILayout.ObjectField(rig.ModelOpenedForEdit, typeof(MinecraftModel), false, options);
		if (changedModel != rig.ModelOpenedForEdit)
			rig.Button_OpenModel(AssetDatabase.GetAssetPath(changedModel));
	}

	private void AnimationButton(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Object changedAnim = EditorGUILayout.ObjectField(rig.SelectedAnimation, typeof(AnimationDefinition), false, options);
		if (changedAnim != rig.SelectedAnimation)
			rig.Button_ApplyAnimation(AssetDatabase.GetAssetPath(changedAnim));
	}

	private static readonly string[] APDefaults = new string[] {
		"NotAttached",
		"DefaultPose",
		"Alex_RightHandPose",
		"Alex_LeftHandPose",
		"Steve_RightHandPose",
		"Steve_LeftHandPose",
		"GUIPose",
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
		if(changedIndex != selectedIndex)
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
			for(int i = 0; i < turbo.AttachPoints.Count; i++)
			{
				AttachPoint ap = turbo.AttachPoints[i];
				apNames.Add(ap.name);
				if (ap.name == parent.name)
					attachedTo = i+1;
			}
			int changedAttachedTo = EditorGUILayout.Popup(attachedTo, apNames.ToArray(), options);
			if(changedAttachedTo != attachedTo)
			{
				Transform newParent = parentRig.transform;
				if(changedAttachedTo != 0)
				{
					newParent = parentRig.transform.FindRecursive(turbo.AttachPoints[changedAttachedTo - 1].name);
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


	private MinecraftModel UpdateModel(Model model, ContentPack pack, Definition def)
	{
		switch (model.Type)
		{
			case Model.ModelType.TurboRig:
			{
				TurboRig rig = CreateInstance<TurboRig>();
				foreach(Model.Section modelSection in model.sections)
				{
					TurboModel section = new TurboModel();
					section.PartName = Utils.ConvertPartName(modelSection.partName);
					section.Pieces = new List<TurboPiece>();
					for (int i = 0; i < modelSection.pieces.Length; i++)
						section.Pieces.Add( modelSection.pieces[i].CopyAsTurbo());
					rig.Sections.Add(section);
				}
				foreach (Model.AnimationParameter animParam in model.animations)
					rig.AnimationParameters.Add(new AnimationParameter(animParam.key, animParam.isVec3, animParam.floatValue, animParam.vec3Value));
				foreach (Model.AttachPoint attachPoint in model.attachPoints)
					rig.AttachPoints.Add(new AttachPoint(
						Utils.ConvertPartName(attachPoint.name), 
						Utils.ConvertPartName(attachPoint.attachedTo), 
						attachPoint.position));

				rig.GetOrCreate("barrel");
				rig.GetOrCreate("slide");
				rig.GetOrCreate("ammo_0");
				rig.GetOrCreate("stock");
				rig.GetOrCreate("sights");
				rig.GetOrCreate("grip");


				rig.TextureX = model.textureX;
				rig.TextureY = model.textureY;

				if(def.Skin != null)
					rig.AddTexture("default", pack.ModName, def.Skin);
				if(def is GunDefinition gun)
				{
					foreach(PaintjobDefinition paint in gun.paints.paintjobs)
					{
						rig.AddTexture(paint.textureName, pack.ModName, def.GetSkin(paint.textureName));
					}
				}
				return rig;
			}
			case Model.ModelType.Block:
			{
				CubeModel cube = CreateInstance<CubeModel>();
				cube.north = new ResourceLocation(model.north);
				cube.east = new ResourceLocation(model.east);
				cube.south = new ResourceLocation(model.south);
				cube.west = new ResourceLocation(model.west);
				cube.top = new ResourceLocation(model.top);
				cube.bottom = new ResourceLocation(model.bottom);
				cube.particle = new ResourceLocation(model.north);
				return cube;
			}
			case Model.ModelType.Custom:
			{
				BlockbenchModel bbModel = CreateInstance<BlockbenchModel>();
				return bbModel;
			}
			case Model.ModelType.Item:
			{
				ItemModel itemModel = CreateInstance<ItemModel>();
				return itemModel;
			}
		}
		return null;
	}
}
