using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModelEditingRig))]
public class ModelEditingRigEditor : Editor
{
	private Tab CurrentTab = Tab.Models;
	private enum Tab
	{
		Models,
		Animations,
		Skins,
	}
	private static readonly string[] TabNames = new string[] {
		"Model",
		"Animations",
		"Skin",
	};

	public override void OnInspectorGUI()
	{
		ModelEditingRig rig = (ModelEditingRig)target;
		if (rig == null)
			return;

		CurrentTab = (Tab)GUILayout.Toolbar((int)CurrentTab, TabNames);
		switch (CurrentTab)
		{
			case Tab.Models:
				ModelsTab(rig);
				break;
			case Tab.Animations:
				AnimationsTab(rig);
				break;
			case Tab.Skins:
				SkinsTab(rig);
				break;
		}
	}

	// ------------------------------------------------------------------------
	#region Modelling Tab
	// ------------------------------------------------------------------------
	private Editor ModelSubEditor = null;
	private bool ModelEditorFoldout = false;
	private void ModelsTab(ModelEditingRig rig)
	{
		if (rig == null)
			return;

		ModelButton(rig);
		if (rig.ModelOpenedForEdit != null)
		{
			bool dirty = EditorUtility.IsDirty(rig.ModelOpenedForEdit);
			GUILayout.Label(dirty ? $"*{rig.ModelOpenedForEdit.name} has unsaved changes" : $"{rig.ModelOpenedForEdit.name} has no changes.");
		}

		EditorGUI.BeginDisabledGroup(rig.ModelOpenedForEdit == null);
		//ModelEditorFoldout = EditorGUILayout.Foldout(ModelEditorFoldout, "Model Editor");
		//if (ModelEditorFoldout)
		{
			if (ModelSubEditor == null || ModelSubEditor.target != rig.ModelOpenedForEdit)
			{
				ModelSubEditor = Editor.CreateEditor(rig.ModelOpenedForEdit);
			}
			if (ModelSubEditor != null)
			{
				ModelSubEditor.OnInspectorGUI();
			}
		}
		EditorGUI.EndDisabledGroup();
	}
	private void ModelButton(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Object changedModel = EditorGUILayout.ObjectField(rig.ModelOpenedForEdit, typeof(MinecraftModel), false, options);
		if (changedModel != rig.ModelOpenedForEdit)
			rig.OpenModel(changedModel as MinecraftModel);
	}

	#endregion
	// ------------------------------------------------------------------------

	// ------------------------------------------------------------------------
	#region Animation Tab
	// ------------------------------------------------------------------------
	private Editor AnimationSubEditor = null;
	private bool AnimationEditorFoldout = false;
	private const int PREVIEW_INDEX_COL_X = 20;
	private const int PREVIEW_DROPDOWN_COL_X = 128;
	private const int PREVIEW_DURATION_COL_X = 40;
	private const int PREVIEW_REMOVE_COL_X = 20;
	private void AnimationsTab(ModelEditingRig rig)
	{
		if (rig == null)
			return;

		rig.ApplyAnimation = GUILayout.Toggle(rig.ApplyAnimation, "Preview Animations");

		EditorGUI.BeginDisabledGroup(!rig.ApplyAnimation);
		AnimationButton(rig);
		if (rig.SelectedAnimation != null)
		{
			bool dirty = EditorUtility.IsDirty(rig.SelectedAnimation);
			GUILayout.Label(dirty ? $"*{rig.SelectedAnimation.name} has unsaved changes" : $"{rig.SelectedAnimation.name} has no changes.");
		}

		GUILayout.BeginHorizontal();
		if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey")))
			rig.PressBack();
		if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton")))
			rig.PressPlay();
		if (GUILayout.Button(EditorGUIUtility.IconContent("PauseButton")))
			rig.PressPause();

		rig.Looping = GUILayout.Toggle(rig.Looping, "Repeat");
		rig.StepThrough = GUILayout.Toggle(rig.StepThrough, "Step-by-Step");
		GUILayout.EndHorizontal();

		List<string> keyframeNames = new List<string>();
		List<string> sequenceNames = new List<string>();
		List<string> allNames = new List<string>();
		if (rig.SelectedAnimation != null)
		{
			foreach (KeyframeDefinition keyframeDef in rig.SelectedAnimation.keyframes)
			{
				keyframeNames.Add(keyframeDef.name);
				allNames.Add($"Keyframe:{keyframeDef.name}");
			}
			foreach (SequenceDefinition sequenceDef in rig.SelectedAnimation.sequences)
			{
				sequenceNames.Add(sequenceDef.name);
				allNames.Add($"Sequence:{sequenceDef.name}");
			}
		}

		float animProgress = rig.GetPreviewProgressSeconds();
		float animDuration = rig.GetPreviewDurationSeconds();

		GUILayout.Label($"[*] Previews ({animProgress.ToString("0.00")}/{animDuration.ToString("0.00")})");

		int previewIndex = -1;
		float animParameter = 0.0f;
		ModelEditingRig.AnimPreviewEntry currentPreview = rig.GetCurrentPreviewEntry(out previewIndex, out animParameter);
		int indexToRemove = -1;

		// ------------------------------------------------------------------------
		// For each entry, render the settings and mark it in green/bold if current
		for (int i = 0; i < rig.PreviewSequences.Count; i++)
		{
			GUILayout.BeginHorizontal();
			ModelEditingRig.AnimPreviewEntry entry = rig.PreviewSequences[i];
			float previewDurationSeconds = rig.GetDurationSecondsOf(entry);
			int previewDurationTicks = rig.GetDurationTicksOf(entry);

			float currentSeconds = animParameter * previewDurationSeconds;
			int currentTicks = Mathf.FloorToInt(currentSeconds * 20f);
			if (entry == currentPreview)
				FlanStyles.SelectedLabel($"[{i + 1}]", GUILayout.Width(PREVIEW_INDEX_COL_X));
			else
				GUILayout.Label($"[{i + 1}]", GUILayout.Width(PREVIEW_INDEX_COL_X));
			string compactName = $"{(entry.IsSequence ? "Sequence" : "Keyframe")}:{entry.Name}";
			int selectedIndex = allNames.IndexOf(compactName);
			int modifiedIndex = EditorGUILayout.Popup(selectedIndex, allNames.ToArray(), GUILayout.Width(PREVIEW_DROPDOWN_COL_X));
			if (selectedIndex != modifiedIndex)
			{
				entry.IsSequence = allNames[modifiedIndex].Contains("Sequence:");
				entry.Name = allNames[modifiedIndex];
				entry.Name = entry.Name.Substring(entry.Name.IndexOf(":") + 1);
			}

			EditorGUI.BeginDisabledGroup(entry.IsSequence);
			int durationTicks = rig.GetDurationTicksOf(entry);
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
			rig.PreviewSequences.RemoveAt(indexToRemove);

		// ----------------------------------------------------------------------
		// Extra element that gets "selected" if you are at the end, also an add button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("+", GUILayout.Width(PREVIEW_INDEX_COL_X)))
			rig.PreviewSequences.Add(new ModelEditingRig.AnimPreviewEntry()
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
		for (int i = 0; i < rig.PreviewSequences.Count; i++)
		{
			float previewSeconds = rig.GetDurationSecondsOf(i);
			float guiWidth = Screen.width * previewSeconds / animDuration;
			if (GUILayout.Button(rig.PreviewSequences[i].Name, FlanStyles.BorderlessButton, GUILayout.Width(guiWidth)))
			{
				rig.SetPreviewIndex(i);
			}
		}
		GUILayout.EndHorizontal();
		float edited = GUILayout.HorizontalSlider(animProgress, 0f, animDuration);
		if (!Mathf.Approximately(edited, animProgress))
			rig.SetPreviewProgressSeconds(edited);
		GUILayout.Space(16);
		// ----------------------------------------------------------------------

		AnimationEditorFoldout = EditorGUILayout.Foldout(AnimationEditorFoldout, "Animation Editor");
		if (AnimationEditorFoldout)
		{
			if (AnimationSubEditor == null || AnimationSubEditor.target != rig.SelectedAnimation)
			{
				AnimationSubEditor = Editor.CreateEditor(rig.SelectedAnimation);
			}
			if (AnimationSubEditor != null)
			{
				AnimationSubEditor.OnInspectorGUI();
			}
		}
		EditorGUI.EndDisabledGroup();
	}
	private void AnimationButton(ModelEditingRig rig, params GUILayoutOption[] options)
	{
		Object changedAnim = EditorGUILayout.ObjectField(rig.SelectedAnimation, typeof(AnimationDefinition), false, options);
		if (changedAnim != rig.SelectedAnimation)
			rig.OpenAnimation(changedAnim as AnimationDefinition);
	}
	#endregion
	// ------------------------------------------------------------------------

	// ------------------------------------------------------------------------
	#region Skins Tab
	// ------------------------------------------------------------------------
	private static readonly List<float> TextureZoomSettings = new List<float>(new float[] { 0, 1, 2, 4, 8, 16, 32, 64 });
	private static readonly string[] TextureZoomSettingNames = new string[] { "Auto", "1", "2", "4", "8", "16", "32", "64" };
	private List<int> ExpandedTextures = new List<int>();
	private List<string> SkinNodeFoldouts = new List<string>();
	private Vector2 TexturePreviewScroller = Vector2.zero;
	private void SkinsTab(ModelEditingRig rig)
	{
		if (rig == null)
			return;

		GUILayout.Label("Texture Zoom Level", FlanStyles.BoldLabel);
		int oldIndex = TextureZoomSettings.IndexOf(MinecraftModelPreview.TextureZoomLevel);
		int newIndex = GUILayout.Toolbar(oldIndex, TextureZoomSettingNames);
		MinecraftModelPreview.TextureZoomLevel = TextureZoomSettings[newIndex];

		FlanStyles.BigHeader("Skins");
		rig.ApplySkin = GUILayout.Toggle(rig.ApplySkin, "Apply Skin");
		GUILayout.Label(rig.SelectedSkin);
		if (rig.Preview is TurboRigPreview turboPreview)
		{
			// Draw a box for each texture
			for (int i = 0; i < turboPreview.Rig.Textures.Count; i++)
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
				GUILayout.Label($"[{i}] {texture.Key}", GUILayout.Width(200));

				if (!newExpanded)
				{
					GUIVerify.VerificationIcon(verifications);
					ResourceLocation changedTextureLocation = ResourceLocation.EditorObjectField<Texture2D>(texture.Location, "textures/skins");
					if (changedTextureLocation != texture.Location)
					{
						texture.Location = changedTextureLocation;
						texture.Texture = changedTextureLocation.Load<Texture2D>();
					}
				}
				GUILayout.FlexibleSpace();

				GUILayout.EndHorizontal();

				if (newExpanded)
				{
					GUILayout.BeginHorizontal();

					GUILayout.Box(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 15));

					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Key: ");
					texture.Key = GUILayout.TextField(texture.Key);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Texture: ");
					ResourceLocation changedTextureLocation = ResourceLocation.EditorObjectField<Texture2D>(texture.Location, "textures/skins");
					if (changedTextureLocation != texture.Location)
					{
						texture.Location = changedTextureLocation;
						texture.Texture = changedTextureLocation.Load<Texture2D>();
					}
					GUILayout.EndHorizontal();

					if (texture.Texture != null)
					{
						TexturePreviewScroller = GUILayout.BeginScrollView(TexturePreviewScroller, GUILayout.ExpandHeight(false));
						RenderTextureAutoWidth(texture.Texture);
						GUILayout.EndScrollView();
					}
					GUIVerify.VerificationsBox(verifications);
					GUILayout.EndVertical();

					GUILayout.EndHorizontal();
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
		InitialSkinNode(rig.Preview);
	}
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
	#endregion
	// ------------------------------------------------------------------------
}
