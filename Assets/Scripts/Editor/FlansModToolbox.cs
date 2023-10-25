using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;

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
		ContentPacks,
		Rigs,
	}
	private static readonly string[] TabNames = new string[]
	{
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

		SelectedTab = (Tab)GUILayout.Toolbar((int)SelectedTab, TabNames);
		switch (SelectedTab)
		{
			case Tab.ContentPacks:
				ContentPacksTab();
				break;
			case Tab.Rigs:
				RigsTab();
				break;
		}

		GUILayout.EndScrollView();
	}

	private int SelectedContentPackIndex = -1;
	private string SelectedContentPackName { get { return SelectedContentPackIndex >= 0 ? Packs[SelectedContentPackIndex].ModName : "None"; } }
	private ContentPack SelectedContentPack { get { return SelectedContentPackIndex >= 0 ? Packs[SelectedContentPackIndex] : null; } }

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
	}

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

	private void SkinsTab()
	{
		if (SelectedRig == null)
			return;
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

		/*
		if (GUILayout.Button("Run"))
		{
			foreach(string file in Directory.EnumerateFiles(recipeFolder))
			{
				
				if(file.Contains($"part_fab_{copyFromMat}"))
				{
					string jsonText = File.ReadAllText(file);
					jsonText = jsonText.Replace(copyFromMat, copyToMat);
					File.WriteAllText(file.Replace(copyFromMat, copyToMat), jsonText);
					Debug.Log($"Writing to {file.Replace(copyFromMat, copyToMat)}");
				}
			}
		}


		if(DefinitionImporter != null)
		{
			// Pack selector
			GUILayout.Label("Select Content Pack");
			List<string> packNames = new List<string>();
			packNames.Add("None");
			int selectedPackIndex = 0;
			for(int i = 0; i < DefinitionImporter.Packs.Count; i++)
			{
				ContentPack pack = DefinitionImporter.Packs[i];
				packNames.Add(pack.ModName);
				if(pack.ModName == SelectedContentPackName)
				{
					selectedPackIndex = i + 1;
				}
			}
			selectedPackIndex = EditorGUILayout.Popup(selectedPackIndex, packNames.ToArray());
			SelectedContentPackName = selectedPackIndex == 0 ? "" : DefinitionImporter.Packs[selectedPackIndex - 1].ModName;
			GUILayout.Label(" ------------ ");
			// ---


			if(selectedPackIndex >= 1)
			{
				ContentPack pack = DefinitionImporter.Packs[selectedPackIndex - 1];
				
			}

			if (GUILayout.Button("Update Models"))
			{
				foreach (ContentPack pack in DefinitionImporter.Packs)
				{
					string modelsPath = $"{DefinitionImporter.ASSET_ROOT}/{pack.ModName}/models";
					if (!Directory.Exists(modelsPath))
						Directory.CreateDirectory(modelsPath);
					foreach (Definition def in pack.Content)
					{
						if(def.Model != null)
						{
							MinecraftModel updatedModel = UpdateModel(def.Model, pack, def);
							if(updatedModel != null)
							{
								updatedModel.name = def.name;
								if (updatedModel.name == null || updatedModel.name.Length == 0)
								{
									updatedModel.name = "unknown";
								}
								AssetDatabase.CreateAsset(updatedModel, $"{modelsPath}/{updatedModel.name}.asset");
							}
						}
					}
				}
			}
		}

		if(ModelPreviewerInst != null)
		{
			GUILayout.Label("Test");
		}
		*/

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
