using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static MinecraftModel;

[ExecuteInEditMode]
public class ModelEditingRig : MonoBehaviour
{
	// ------------------------------------------------------------------------------------------
	#region General
	// ------------------------------------------------------------------------------------------
	[HideInInspector]
	public MinecraftModel ModelOpenedForEdit = null; 
	public string ModelName { get { return ModelOpenedForEdit == null ? "None" : ModelOpenedForEdit.name; } }
	public void OpenModel(MinecraftModel model)
	{
		CloseModel();

		ModelOpenedForEdit = model;

		InitializePreviews();
		ApplySkinOnOpenModel();
	}
	public void CloseModel()
	{
		// Unparent any rigs attached to this
		ModelEditingRig[] subRigs = GetComponentsInChildren<ModelEditingRig>();
		foreach(ModelEditingRig subRig in subRigs)
		{
			if(subRig != this)
				subRig.transform.SetParent(null);
		}
		InvalidatePreviews();
		DiscardTemporaryUVMap();
		ModelOpenedForEdit = null;
	}
	public void OnEnable()
	{
		EditorApplication.update += EditorUpdate;
	}
	public void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
	}
	public void EditorUpdate()
	{
		UpdateAnims();
	}

	#endregion
	// ------------------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------------------
	#region Previewers
	// ------------------------------------------------------------------------------------------
	[SerializeField]
	private MinecraftModelPreview _Preview = null;
	public MinecraftModelPreview Preview 
	{ 
		get
		{
			if (ModelOpenedForEdit != null && _Preview == null)
			{
				if (ModelOpenedForEdit is CubeModel cubeModel)
					_Preview = CreatePreviewObject(cubeModel, false);
				else if (ModelOpenedForEdit is ItemModel itemModel)
					_Preview = CreatePreviewObject(itemModel, false);
				else if (ModelOpenedForEdit is TurboRig turboRig)
					_Preview = CreatePreviewObject(turboRig, false);
			}
			if(ModelOpenedForEdit == null && _Preview != null)
			{
				Debug.LogError($"Closed model without cleaning up Preview objects");
				InvalidatePreviews();
			}
			return _Preview;
		} 
	}
	private void InitializePreviews()
	{
		if(Preview != null)
		{
			Preview.InitializePreviews();
		}
	}
	private void InvalidatePreviews()
	{
		// Delete all our preview objects
		MinecraftModelPreview child = GetComponentInChildren<MinecraftModelPreview>();
		while (child != null) // Weird loop because they might be parented
		{
			DestroyImmediate(child.gameObject);
			child = GetComponentInChildren<MinecraftModelPreview>();
		}
	}

	private TurboModelPreview FindSectionPreview(string key)
	{
		foreach (TurboModelPreview section in GetComponentsInChildren<TurboModelPreview>())
			if (section.PartName == key)
				return section;
		return null;
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
			preview.RefreshGeometry();
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
	#endregion
	// ------------------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------------------
	#region Skins
	// ------------------------------------------------------------------------------------------
	[Header("Skins")]
    public bool ApplySkin = true;
	public string SelectedSkin = "debug";
	[NonSerialized]
	public UVMap TemporaryUVMap = null;
	public Vector2Int UVMapSize 
	{ 
		get 
		{
			if (TemporaryUVMap != null)
				return TemporaryUVMap.MaxSize;
			else return Vector2Int.zero;
		 } 
	}
	public Material SkinMaterial 
	{
		get
		{
			if (_SkinMaterial == null || _SkinMaterial.name != name)
			{
				_SkinMaterial = new Material(Shader.Find("Standard"));
				_SkinMaterial.name = name;
				_SkinMaterial.SetTexture("_MainTex", null);
				_SkinMaterial.EnableKeyword("_NORMALMAP");
				_SkinMaterial.EnableKeyword("_DETAIL_MULX2");
				_SkinMaterial.SetOverrideTag("RenderType", "Cutout");
			}
			return _SkinMaterial;
		}
	}
	[SerializeField]
	private Material _SkinMaterial = null;
	public Texture2D DebugTexture
	{
		get
		{
			if (_DebugTexture == null)
			{
				_DebugTexture = new Texture2D(UVMapSize.x, UVMapSize.y);
				_DebugTexture.filterMode = FilterMode.Point;
			}
			RegenerateDebugTexture();
			return _DebugTexture;
		}
	}
	[SerializeField]
	private Texture2D _DebugTexture = null;
	private void RegenerateDebugTexture()
	{
		SkinGenerator.CreateDefaultTexture(TemporaryUVMap, _DebugTexture);
	}
	public void CreateTemporaryUVMap()
	{
		if(ModelOpenedForEdit != null)
		{
			TemporaryUVMap = ModelOpenedForEdit.BakedUVMap.Clone();
			TemporaryUVMap.CalculateBounds();
		}
	}
	public void DiscardTemporaryUVMap()
	{
		TemporaryUVMap = null;
	}
	public UVMap GetPreviewUVMap()
	{
		if (TemporaryUVMap != null)
			return TemporaryUVMap;
		if (ModelOpenedForEdit != null)
			return ModelOpenedForEdit.BakedUVMap;
		return null;
	}
	public bool TryGetNamedTexture(string skinKey, out NamedTexture namedTexture)
	{
		if (ModelOpenedForEdit != null)
		{
			namedTexture = ModelOpenedForEdit.GetNamedTexture(skinKey);
			return namedTexture != null;
		}
		namedTexture = null;
		return false;
	}
	public Texture2D GetTextureForSkinKey(string skinKey)
	{
		if (TryGetNamedTexture(skinKey, out NamedTexture namedTexture))
			return namedTexture.Texture;

		return DebugTexture;
	}
	public void ApplySkinOnOpenModel()
	{
		if (ModelOpenedForEdit.Textures.Count > 0)
			SelectSkin(ModelOpenedForEdit.Textures[0].Key);
		else
			SelectDebugTexture();
	}
	public void SelectSkin(string skinKey)
	{
		SelectedSkin = skinKey;
		Texture2D skinTexture = GetTextureForSkinKey(skinKey);
		SkinMaterial.SetTexture("_MainTex", skinTexture);
	}
	public void SelectDebugTexture()
	{
		SelectedSkin = "debug";
		SkinMaterial.SetTexture("_MainTex", DebugTexture);
	}
	public void SetPreivewUVMap(UVMap temporaryUVs)
	{
		TemporaryUVMap = temporaryUVs;

		// If we are live-editing UVs, we don't want to rip our textures up, so swap to debug
		if(SelectedSkin != "debug")
		{
			SelectDebugTexture();
		}
		RegenerateDebugTexture();

		foreach (MinecraftModelPreview subPreview in GetComponentsInChildren<MinecraftModelPreview>())
			subPreview.RefreshGeometry();
	}
	#endregion
	// ------------------------------------------------------------------------------------------

	// ------------------------------------------------------------------------------------------
	#region Animations
	// ------------------------------------------------------------------------------------------
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
	public void CloseAnimation()
	{
		SelectedAnimation = null;
	}
	public void OpenAnimation(AnimationDefinition anim)
	{
		CloseAnimation();
		SelectedAnimation = anim;
	}

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
				if (PreviewSequences[i].IsSequence)
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
	#endregion
	// ------------------------------------------------------------------------------------------
}
