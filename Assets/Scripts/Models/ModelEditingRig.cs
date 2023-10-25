using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[ExecuteInEditMode]
public class ModelEditingRig : MonoBehaviour
{
    // Selection
    //[HideInInspector]
    public MinecraftModel ModelOpenedForEdit = null;
    //[HideInInspector]
	//public MinecraftModel WorkingCopy = null;

	public MinecraftModelPreview Preview = null;

	// Currently assuming single texture skinning only
	[Header("Skins")]
    public bool ApplySkin = true;
    public string SelectedSkin = "default";
	public MinecraftModel.NamedTexture GetNamedTexture() 
	{ 
		if(ModelOpenedForEdit != null)
			return ModelOpenedForEdit.GetNamedTexture(SelectedSkin);
		return null;
	}
	public Texture2D GetTexture2D()
	{
		MinecraftModel.NamedTexture namedTexture = GetNamedTexture();
		if (namedTexture != null)
			return namedTexture.Texture;
		return null;
	}
	public string ModelName { get { return ModelOpenedForEdit == null ? "None" : ModelOpenedForEdit.name; } }

	[Header("Animations")]
    public bool ApplyAnimation = false;
    public AnimationDefinition SelectedAnimation = null;
	private DateTime LastEditorTick = DateTime.Now;
	private float AnimProgressSeconds = 0.0f;
	[System.Serializable]
	public class AnimPreviewEntry
	{
		public bool IsSequence = false;
		public string Name = "";
		public int DurationTicks = 0;
	}
	public bool Playing = false;
	public bool Looping = false;
	public bool StepThrough = false;
	public List<AnimPreviewEntry> PreviewSequences = new List<AnimPreviewEntry>();
	public SequenceDefinition GetSequence(string sequenceName)
	{
		if(SelectedAnimation != null)
		{
			foreach (SequenceDefinition seq in SelectedAnimation.sequences)
				if (seq.name == sequenceName)
					return seq;
		}
		return null;
	}
	public KeyframeDefinition GetKeyframe(string keyframeName)
	{
		if (SelectedAnimation != null)
		{
			foreach (KeyframeDefinition keyframe in SelectedAnimation.keyframes)
				if (keyframe.name == keyframeName)
					return keyframe;
		}
		return null;
	}
	public float GetPreviewProgressSeconds()
	{
		return AnimProgressSeconds;
	}
	public int GetPreviewProgressTicks()
	{
		return Mathf.CeilToInt(AnimProgressSeconds * 20f);
	}
	public float GetPreviewDurationSeconds()
	{
		return GetPreviewDurationTicks() / 20f;
	}
	public int GetPreviewDurationTicks()
	{
		int time = 0;
		foreach(AnimPreviewEntry preview in PreviewSequences)
		{
			if(preview.IsSequence)
			{
				SequenceDefinition seq = GetSequence(preview.Name);
				if (seq != null)
					time += seq.ticks;
			}
			else
			{
				time += preview.DurationTicks;
			}
		}
		return time;
	}
	public void SetPreviewProgressTicks(int ticks)
	{
		AnimProgressSeconds = ticks / 20.0f;
	}
	public void SetPreviewProgressSeconds(float seconds)
	{
		AnimProgressSeconds = seconds;
	}
	public void SetPreviewIndex(int index)
	{
		AnimProgressSeconds = 0.0f;
		for (int i = 0; i < index; i++)
			AnimProgressSeconds += GetDurationSecondsOf(PreviewSequences[i]);
	}
	public float GetDurationSecondsOf(int previewIndex)
	{
		return GetDurationTicksOf(PreviewSequences[previewIndex]) / 20f;
	}
	public float GetDurationSecondsOf(AnimPreviewEntry entry)
	{
		return GetDurationTicksOf(entry) / 20f;
	}
	public int GetDurationTicksOf(AnimPreviewEntry entry)
	{
		if (entry.IsSequence)
		{
			SequenceDefinition sequence = FindSequence(entry.Name);
			return GetSequenceLengthTicks(sequence);
		}
		else
		{
			return entry.DurationTicks;
		}
	}
	public AnimPreviewEntry GetCurrentPreviewEntry(out int index, out float parameter)
	{
		float progressInSeconds = GetPreviewProgressSeconds();
		for (int i = 0; i < PreviewSequences.Count; i++)
		{
			float durationSeconds = GetDurationSecondsOf(PreviewSequences[i]); 
			if (progressInSeconds < durationSeconds)
			{
				index = i;
				parameter = progressInSeconds / durationSeconds;
				return PreviewSequences[i];
			}

			progressInSeconds -= durationSeconds;
		}
		index = -1;
		parameter = 0.0f;
		return null;
	}

	public void OnEnable()
	{
		EditorApplication.update += UpdateAnims;
	}

	public void OnDisable()
	{
		EditorApplication.update -= UpdateAnims;
	}

	public void Update()
	{
        RefreshMeshes();
	}
	public void PressPlay()
	{
		if (StepThrough)
			AnimProgressSeconds += 1f / 20f;
		else
			Playing = true;
	}
	public void PressPause()
	{
		Playing = false;
	}
	public void PressBack()
	{
		AnimProgressSeconds = 0.0f;
	}

	public void UpdateAnims()
	{
		if (ApplyAnimation && PreviewSequences.Count > 0)
		{
			AnimPreviewEntry entry = null;

			if (Playing && !StepThrough)
			{
				TimeSpan timeToAdd = DateTime.Now - LastEditorTick;
				AnimProgressSeconds += (float)(timeToAdd.TotalMilliseconds / 1000d);
			}
			LastEditorTick = DateTime.Now;

			float progressInTicks = AnimProgressSeconds * 20f;

			SequenceDefinition sequence = null;
			KeyframeDefinition keyframe = null;

			for (int i = 0; i < PreviewSequences.Count; i++)
			{
				float duration = 0.0f;
				if(PreviewSequences[i].IsSequence)
				{
					sequence = FindSequence(PreviewSequences[i].Name);
					keyframe = null;
					duration = GetSequenceLengthTicks(sequence);
				}
				else
				{
					sequence = null;
					keyframe = FindKeyframe(PreviewSequences[i].Name);
					duration = PreviewSequences[i].DurationTicks;
				}

				if (progressInTicks < duration)
					break;

				progressInTicks -= duration;
				if (i == PreviewSequences.Count - 1)
				{
					if (Looping)
						AnimProgressSeconds -= GetPreviewDurationSeconds();
					else
						AnimProgressSeconds = GetPreviewDurationSeconds();
				}
			}

			if (keyframe != null)
			{
				ApplyPose(keyframe);
			}
			else if (sequence != null)
			{
				SequenceEntryDefinition[] segment = GetSegment(sequence, progressInTicks);
				float segmentDuration = segment[1].tick - segment[0].tick;

				// If it is valid, let's animate it
				if (segmentDuration > 0.0f)
				{
					KeyframeDefinition from = FindKeyframe(segment[0].frame);
					KeyframeDefinition to = FindKeyframe(segment[1].frame);
					if (from != null && to != null)
					{
						float linearParameter = (progressInTicks - segment[0].tick) / segmentDuration;
						linearParameter = Mathf.Clamp(linearParameter, 0f, 1f);
						float outputParameter = linearParameter;

						// Instant transitions take priority first
						if (segment[0].exit == ESmoothSetting.instant)
							outputParameter = 1.0f;
						if (segment[1].entry == ESmoothSetting.instant)
							outputParameter = 0.0f;

						// Then apply smoothing?
						if (segment[0].exit == ESmoothSetting.smooth)
						{
							// Smoothstep function
							if (linearParameter < 0.5f)
								outputParameter = linearParameter * linearParameter * (3f - 2f * linearParameter);
						}
						if (segment[1].entry == ESmoothSetting.smooth)
						{
							// Smoothstep function
							if (linearParameter > 0.5f)
								outputParameter = linearParameter * linearParameter * (3f - 2f * linearParameter);
						}


						foreach (var sectionPreview in GetComponentsInChildren<TurboModelPreview>())
						{
							PoseDefinition fromPose = GetPose(from.name, sectionPreview.PartName);
							PoseDefinition toPose = GetPose(to.name, sectionPreview.PartName);
							Vector3 pos = LerpPosition(fromPose, toPose, outputParameter);
							Quaternion ori = LerpRotation(fromPose, toPose, outputParameter);

							sectionPreview.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
							sectionPreview.transform.localRotation = ori;
							sectionPreview.transform.localScale = Vector3.one;
						}
					}
				}
			}
			else 
			{
				// Found neither matching frame nor sequence, default pose?
				SetDefaultPose();
			}
		}
		else
		{
			SetDefaultPose();
		}
	}
	private void SetDefaultPose()
	{
		foreach (TurboModelPreview section in GetComponentsInChildren<TurboModelPreview>())
		{
			section.transform.localPosition = Vector3.zero;
			section.transform.localRotation = Quaternion.identity;
			section.transform.localScale = Vector3.one;
		}
	}

	private TurboRigPreview FindRigPreview()
	{
		return GetComponentInChildren<TurboRigPreview>();
	}
	private TurboModelPreview FindSectionPreview(string key)
	{
		foreach (TurboModelPreview section in GetComponentsInChildren<TurboModelPreview>())
			if (section.PartName == key)
				return section;
		return null;
	}
	private void ApplyPose(KeyframeDefinition keyframe)
	{
		foreach (PoseDefinition pose in keyframe.poses)
		{
			TurboModelPreview sectionPreview = FindSectionPreview(pose.applyTo);
			if (sectionPreview != null)
			{
				Vector3 pos = Resolve(pose.position);
				Vector3 euler = Resolve(pose.rotation);
				sectionPreview.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
				sectionPreview.transform.localEulerAngles = euler;
				sectionPreview.transform.localScale = pose.scale;
			}
		}
	}
	private Vector3 LerpPosition(PoseDefinition from, PoseDefinition to, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		Vector3 a = from == null ? Vector3.zero : Resolve(from.position);
		Vector3 b = to == null ? Vector3.zero : Resolve(to.position);
		return Vector3.Lerp(a, b, t);
	}
	private PoseDefinition GetPose(string keyframeName, string part)
	{
		KeyframeDefinition keyframe = FindKeyframe(keyframeName);
		if (keyframe != null)
		{
			foreach (PoseDefinition pose in keyframe.poses)
			{
				if (pose.applyTo == part)
					return pose;
			}

			foreach (string parent in keyframe.parents)
			{
				PoseDefinition poseFromParent = GetPose(parent, part);
				if (poseFromParent != null)
					return poseFromParent;
			}
		}
		else
		{
			Debug.LogError($"Could not find keyframe {keyframeName}");
		}

		return null;
	}
	private Quaternion LerpRotation(PoseDefinition from, PoseDefinition to, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		Vector3 a = from == null ? Vector3.zero : Resolve(from.rotation);
		Vector3 b = to == null ? Vector3.zero : Resolve(to.rotation);
		return Quaternion.Slerp(
			Quaternion.Euler(a),
			Quaternion.Euler(b),
			t);
	}
	private Vector3 Resolve(VecWithOverride v)
	{
		return new Vector3((float)v.xValue, (float)v.yValue, (float)v.zValue);
	}
	private SequenceDefinition FindSequence(string key)
	{
		foreach (SequenceDefinition sequence in SelectedAnimation.sequences)
			if (sequence.name == key)
				return sequence;
		return null;
	}
	private int GetSequenceLengthTicks(SequenceDefinition sequenceDefinition)
	{
		if (sequenceDefinition == null)
			return 0;
		int highestTick = 0;
		foreach (SequenceEntryDefinition entry in sequenceDefinition.frames)
		{
			if (entry.tick > highestTick)
				highestTick = entry.tick;
		}
		return highestTick;
	}
	private SequenceEntryDefinition[] GetSegment(SequenceDefinition sequence, float tickPlusPartial)
	{
		SequenceEntryDefinition[] entries = new SequenceEntryDefinition[2];
		entries[0] = sequence.frames[0];
		entries[1] = sequence.frames[sequence.frames.Length - 1];

		for (int i = 0; i < sequence.frames.Length; i++)
		{
			// If this is the closest above or below our current time, set it
			if (sequence.frames[i].tick <= tickPlusPartial && sequence.frames[i].tick > entries[0].tick)
				entries[0] = sequence.frames[i];

			if (sequence.frames[i].tick > tickPlusPartial && sequence.frames[i].tick < entries[1].tick)
				entries[1] = sequence.frames[i];
		}

		return entries;
	}
	private KeyframeDefinition FindKeyframe(string key)
	{
		foreach (KeyframeDefinition keyframe in SelectedAnimation.keyframes)
			if (keyframe.name == key)
				return keyframe;
		return null;
	}

	public void RefreshMeshes()
    {
        if (ModelOpenedForEdit == null)
            return;

		if (ModelOpenedForEdit is CubeModel cubeModel)
		{
			Preview = CreatePreviewObject(cubeModel, false);
		}
		else if (ModelOpenedForEdit is ItemModel itemModel)
		{
			Preview = CreatePreviewObject(itemModel, false);
		}
		else if (ModelOpenedForEdit is TurboRig turboRig)
		{
			Preview = CreatePreviewObject(turboRig, false);
		}
		Preview.Refresh();
	}

	public MinecraftModelPreview CreatePreviewObject(TurboRig model, bool forceUpdate)
	{
        Transform existing = transform.Find("turbo");
        if (existing == null || forceUpdate)
        {
            if (existing != null)
                DestroyImmediate(existing.gameObject);
            GameObject go = new GameObject("turbo");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            TurboRigPreview preview = go.AddComponent<TurboRigPreview>();
			preview.SetModel(model);
			return preview;
        }
		existing.GetComponent<TurboRigPreview>().SetModel(model);
		return existing.GetComponent<TurboRigPreview>();
	}

	public MinecraftModelPreview CreatePreviewObject(CubeModel model, bool forceUpdate)
    {
		Transform existing = transform.Find("cube");
		if (existing == null || forceUpdate)
		{
			if (existing != null)
				DestroyImmediate(existing.gameObject);
			GameObject go = new GameObject("cube");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            CubeModelPreview preview = go.AddComponent<CubeModelPreview>();
            preview.SetModel(model);
            return preview;
		}
		return existing.GetComponent<CubeModelPreview>();
	}

	public MinecraftModelPreview CreatePreviewObject(ItemModel model, bool forceUpdate)
	{
		Transform existing = transform.Find("item");
		if (existing == null || forceUpdate)
		{
			if (existing != null)
				DestroyImmediate(existing.gameObject);
			GameObject go = new GameObject("item");
			go.transform.SetParent(transform);
			go.transform.localPosition = Vector3.zero;
			go.transform.localScale = Vector3.one;
			go.transform.localRotation = Quaternion.identity;
			ItemModelPreview preview = go.AddComponent<ItemModelPreview>();
			preview.SetModel(model);
			return preview;
		}
		return existing.GetComponent<ItemModelPreview>();
	}

	public void OpenModel(MinecraftModel model)
    {
		if (ModelOpenedForEdit != null)
		{
			DiscardChanges();
		}

		ModelOpenedForEdit = model;
        
        RefreshMeshes();
		MinecraftModel.NamedTexture DefaultTexture = model.GetDefaultTexture();
		if(DefaultTexture != null)
			foreach (TurboPiecePreview piece in GetComponentsInChildren<TurboPiecePreview>())
			{
				piece.CopyExistingTexture(DefaultTexture.Texture);
			}
    }

	private void PreSaveStep()
	{
		if(ModelOpenedForEdit != null)
		{
			UVCalculator.AutoUV(ModelOpenedForEdit, Preview, SelectedSkin);
		}
	}

    public void SaveAs(string newAssetPath)
    {
		if (ModelOpenedForEdit != null)
		{
			PreSaveStep();

			// Serialize-copy our model
			MinecraftModel SaveAsCopy = (MinecraftModel)ScriptableObject.CreateInstance(ModelOpenedForEdit.GetType());
			string json = JsonUtility.ToJson(ModelOpenedForEdit);
			JsonUtility.FromJsonOverwrite(json, SaveAsCopy);

            // Save to disk
            AssetDatabase.CreateAsset(SaveAsCopy, newAssetPath);
			EditorUtility.SetDirty(SaveAsCopy);

            // Then re-establish the new connection
            ModelOpenedForEdit = AssetDatabase.LoadAssetAtPath<MinecraftModel>(newAssetPath);
		}
	}

    public void SaveChanges()
    {
        if (ModelOpenedForEdit != null)
        {
			PreSaveStep();

			//string json = JsonUtility.ToJson(WorkingCopy);
            //JsonUtility.FromJsonOverwrite(json, ModelOpenedForEdit);
            //EditorUtility.SetDirty(ModelOpenedForEdit);
            //AssetDatabase.SaveAssets();
			//Debug.Log($"Saved {WorkingCopy} to {AssetDatabase.GetAssetPath(ModelOpenedForEdit)}");
			//
			//IsDirty = false;
        }
        else Debug.LogError("Can't save changes without an open model");
	}

    public void SaveAndCloseModel()
    {
        SaveChanges();
        ModelOpenedForEdit = null;
		foreach (MinecraftModelPreview preview in GetComponentsInChildren<MinecraftModelPreview>())
			if (preview != null)
				DestroyImmediate(preview.gameObject);
	}

    public void DiscardChanges()
    {
        ModelOpenedForEdit = null;
        foreach (MinecraftModelPreview preview in GetComponentsInChildren<MinecraftModelPreview>())
            if (preview != null)
                DestroyImmediate(preview.gameObject);
    }

	public void SaveTexture()
	{
		
	}

	public void SaveTextureAs(string newTexturePath)
	{
		string newTextureName = newTexturePath;
		newTextureName = newTextureName.Substring(
			newTextureName.LastIndexOf('/') + 1,
			newTextureName.LastIndexOf('.') - (newTextureName.LastIndexOf('/') + 1));

		if (UVCalculator.StitchWithExistingUV(ModelOpenedForEdit, Preview, newTextureName))
		{
			Debug.Log($"Saved new skin as {newTextureName}:'{newTexturePath}'");
		}
		else Debug.LogError($"Failed to save new skin as {newTextureName} at {newTexturePath}");
	}

	#if UNITY_EDITOR
	public void Editor_Toolbox()
    {
		if (GUILayout.Button("Load..."))
			Button_OpenModel();

		EditorGUI.BeginDisabledGroup(ModelOpenedForEdit == null);
		{
			if (GUILayout.Button("Save"))
				Button_Save();
			if (GUILayout.Button("Save As..."))
				Button_SaveAs();
			if (GUILayout.Button("Discard"))
				Button_Discard();
		}
		EditorGUI.EndDisabledGroup();
	}

	public void Button_ApplyAnimation(string loadPath = null)
	{
		if (SelectedAnimation != null)
		{
			// Maybe?
		}

		if (loadPath == null)
			loadPath = EditorUtility.OpenFilePanelWithFilters("", "Assets/Content Packs", new string[] { "Imported Animation", "asset" });
		if (loadPath != null && loadPath.Length > 0)
		{
			loadPath = loadPath.Substring(loadPath.IndexOf("Assets"));
			AnimationDefinition anim = AssetDatabase.LoadAssetAtPath<AnimationDefinition>(loadPath);
			if(anim != null)
				SelectedAnimation = anim;
			else Debug.LogError($"Could not load animation at {loadPath}");
		}
	}


	public void Button_OpenModel(string loadPath = null)
    {
		bool canLoad = true;
		if (ModelOpenedForEdit != null && EditorUtility.IsDirty(ModelOpenedForEdit))
		{
			int result = EditorUtility.DisplayDialogComplex(
				"Unsaved changes",
				"Your model has unsaved changes, do you want to save?",
				"Save Changes",
				"Don't Save",
				"Cancel");

			if (result == 0)
				SaveAndCloseModel();
			else if (result == 1)
				DiscardChanges();
			else if (result == 2)
				canLoad = false;
		}

		if (canLoad)
		{
			if(loadPath == null)
				loadPath = EditorUtility.OpenFilePanelWithFilters("", "Assets/Content Packs", new string[] { "Imported Model", "asset" });
			if (loadPath != null && loadPath.Length > 0)
			{
				loadPath = loadPath.Substring(loadPath.IndexOf("Assets"));
				MinecraftModel model = AssetDatabase.LoadAssetAtPath<MinecraftModel>(loadPath);
				if (model != null)
					OpenModel(model);
				else
				{
					Debug.LogError($"Could not load model at {loadPath}");
                    UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(loadPath);
					for (int i = 0; i < allAssets.Length; i++)
						Debug.LogWarning($"Found asset {allAssets[i]}");
				}
			}
		}
	}

    public void Button_Save()
	{
		if (ModelOpenedForEdit != null && EditorUtility.IsDirty(ModelOpenedForEdit))
		{
			if(!ModelOpenedForEdit.IsUVMapSame(ModelOpenedForEdit))
			{
				if (!EditorUtility.DisplayDialog("Model UV Mapping Changed!", 
					$"Are you sure you want to save {ModelOpenedForEdit.name}? The model UV has changed, so skins other than the currently active one will be invalid.", "Yes", "No"))
					return;
			}
			EditorGUI.BeginDisabledGroup(EditorUtility.IsDirty(ModelOpenedForEdit));
			SaveChanges();
			EditorGUI.EndDisabledGroup();
		}
	}

    public void Button_SaveAs()
    {
		if (ModelOpenedForEdit != null)
		{
			string savePath = EditorUtility.SaveFilePanelInProject(ModelOpenedForEdit.name, "new_model", "asset", "Save Model As...");
			if (savePath != null && savePath.Length > 0)
				SaveAs(savePath);
		}
	}

    public void Button_Discard()
    {
		if (ModelOpenedForEdit != null)
		{
			if (EditorUtility.DisplayDialog("Are you sure?", $"Are you sure you want to discard changes to {ModelOpenedForEdit.name}", "Yes", "No"))
				DiscardChanges();
		}
	}

	public void Button_CreateNewTexture()
	{
		
	}

	public void Button_SaveTexture()
	{
		if (EditorUtility.DisplayDialog("Model UV Mapping Changed!", $"Are you sure you want to save {SelectedSkin}? The model UV has changed, so other skins will not match the mapping.", "Yes", "No"))
			SaveTexture();
	}

	public void Button_SaveTextureAs()
	{
		if (ModelOpenedForEdit != null && SelectedSkin.Length > 0)
		{
			string savePath = EditorUtility.SaveFilePanelInProject(SelectedSkin, "new_skin", "asset", "Save Skin As...");
			if (savePath != null && savePath.Length > 0)
				SaveTextureAs(savePath);
		}
	}
    #endif
}
