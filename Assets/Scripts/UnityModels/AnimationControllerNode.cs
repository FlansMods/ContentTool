using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AnimationControllerNode : Node
{
	public FlanimationDefinition SelectedAnimation = null;
	public bool ApplyAnimation = false; 
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
		if (SelectedAnimation != null)
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
		foreach (AnimPreviewEntry preview in PreviewSequences)
		{
			if (preview.IsSequence)
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
		return time == 0 ? 1 : time;
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


	// --------------------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
	// --------------------------------------------------------------------------------------------------------
	private Editor AnimationSubEditor = null;
	private bool AnimationEditorFoldout = false;
	private const int PREVIEW_INDEX_COL_X = 20;
	private const int PREVIEW_DROPDOWN_COL_X = 128;
	private const int PREVIEW_DURATION_COL_X = 40;
	private const int PREVIEW_REMOVE_COL_X = 20;

	private void AnimationSelectorGUI()
	{
		FlanimationDefinition changedAnimSet = (FlanimationDefinition)EditorGUILayout.ObjectField(SelectedAnimation, typeof(FlanimationDefinition), false);
		if (changedAnimSet != SelectedAnimation)
		{
			Undo.RecordObject(this, $"Selected anim set {changedAnimSet.name}");
			SelectedAnimation = changedAnimSet;
			EditorUtility.SetDirty(this);
		}
	}
	private void AnimationControlsGUI()
	{
		if (GUILayout.Button(FlanStyles.ResetToDefault))
			PressBack();
		if (GUILayout.Button(FlanStyles.Play))
			PressPlay();
		if (GUILayout.Button(FlanStyles.Pause))
			PressPause();
		Looping = GUILayout.Toggle(Looping, "Repeat");
		StepThrough = GUILayout.Toggle(StepThrough, "Step-by-Step");
	}
	private void AnimationPreviewsGUI()
	{
		List<string> keyframeNames = new List<string>();
		List<string> sequenceNames = new List<string>();
		List<string> allNames = new List<string>();
		foreach (KeyframeDefinition keyframeDef in SelectedAnimation.keyframes)
		{
			keyframeNames.Add(keyframeDef.name);
			allNames.Add($"Keyframe:{keyframeDef.name}");
		}
		foreach (SequenceDefinition sequenceDef in SelectedAnimation.sequences)
		{
			sequenceNames.Add(sequenceDef.name);
			allNames.Add($"Sequence:{sequenceDef.name}");
		}
		float animProgress = GetPreviewProgressSeconds();
		float animDuration = GetPreviewDurationSeconds();

		GUILayout.Label($"[*] Previews ({animProgress.ToString("0.00")}/{animDuration.ToString("0.00")})");

		int previewIndex = -1;
		float animParameter = 0.0f;
		AnimPreviewEntry currentPreview = GetCurrentPreviewEntry(out previewIndex, out animParameter);
		int indexToRemove = -1;

		// ------------------------------------------------------------------------
		// For each entry, render the settings and mark it in green/bold if current
		for (int i = 0; i < PreviewSequences.Count; i++)
		{
			GUILayout.BeginHorizontal();
			AnimPreviewEntry entry = PreviewSequences[i];
			float previewDurationSeconds = GetDurationSecondsOf(entry);
			int previewDurationTicks = GetDurationTicksOf(entry);

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
			int durationTicks = GetDurationTicksOf(entry);
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
			PreviewSequences.RemoveAt(indexToRemove);

		// ----------------------------------------------------------------------
		// Extra element that gets "selected" if you are at the end, also an add button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("+", GUILayout.Width(PREVIEW_INDEX_COL_X)))
			PreviewSequences.Add(new AnimPreviewEntry()
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
	}
	private void AnimationSliderGUI()
	{
		float animProgress = GetPreviewProgressSeconds();
		float animDuration = GetPreviewDurationSeconds();

		GUILayout.BeginHorizontal();
		for (int i = 0; i < PreviewSequences.Count; i++)
		{
			float previewSeconds = GetDurationSecondsOf(i);
			float guiWidth = (Screen.width - 32) * previewSeconds / animDuration;
			if (GUILayout.Button(PreviewSequences[i].Name, FlanStyles.BorderlessButton, GUILayout.Width(guiWidth)))
			{
				SetPreviewIndex(i);
			}
		}
		GUILayout.EndHorizontal();
		float edited = GUILayout.HorizontalSlider(animProgress, 0f, animDuration);
		if (!Mathf.Approximately(edited, animProgress))
			SetPreviewProgressSeconds(edited);
		GUILayout.Space(16);
	}
	private void AnimationDefSubEditorGUI()
	{
		AnimationEditorFoldout = EditorGUILayout.Foldout(AnimationEditorFoldout, "Animation Editor");
		if (AnimationEditorFoldout)
		{
			if (AnimationSubEditor == null || AnimationSubEditor.target != SelectedAnimation)
			{
				AnimationSubEditor = Editor.CreateEditor(SelectedAnimation);
			}
			if (AnimationSubEditor != null)
			{
				AnimationSubEditor.OnInspectorGUI();
			}
		}
	}
	private void AnimationGUI()
	{
		bool dirty = EditorUtility.IsDirty(SelectedAnimation);
		GUILayout.Label(dirty ? $"*{SelectedAnimation.name} has unsaved changes" : $"{SelectedAnimation.name} has no changes.");

		// Controls Header
		GUILayout.BeginHorizontal();
		AnimationControlsGUI();
		GUILayout.EndHorizontal();

		// Preview series editor
		AnimationPreviewsGUI();

		// Timeline slider
		AnimationSliderGUI();

		AnimationDefSubEditorGUI();
	}

	public override bool HasCompactEditorGUI() { return true; }
    public override void CompactEditorGUI()
    {
        base.CompactEditorGUI();
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();

		ApplyAnimation = GUILayout.Toggle(ApplyAnimation, "Preview Animations");

		EditorGUI.BeginDisabledGroup(!ApplyAnimation);
		AnimationSelectorGUI();

		if (SelectedAnimation != null)
		{
			AnimationGUI();
		}

		EditorGUI.EndDisabledGroup(); // if(!ApplyAnimation)


		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

#endif
	// --------------------------------------------------------------------------------------------------------


	public override void GetVerifications(List<Verification> verifications)
	{
		base.GetVerifications(verifications);

		if(SelectedAnimation != null)
		{
			ResourceLocation resLoc = Root.GetLocation();
			ContentPack pack = ContentManager.inst.FindContentPack(resLoc.Namespace);
			if (pack != null)
			{
				if (pack.TryGetContent(resLoc.IDWithoutPrefixes(), out Definition result))
				{
					if (result is GunDefinition gunDef)
					{
						ResourceLocation animLoc = SelectedAnimation.GetLocation();
						if (gunDef.animationSet != $"{animLoc.Namespace}:{animLoc.IDWithoutPrefixes()}")
						{
							GUILayout.Label($"This animation set '{animLoc}' is not applied to the GunDefinition");
							if (GUILayout.Button("Apply"))
							{
								gunDef.animationSet = $"{animLoc.Namespace}:{animLoc.IDWithoutPrefixes()}";
								EditorUtility.SetDirty(gunDef);
							}
						}
					}
				}
			}
		}
	}
	public void UpdateAnims()
	{
		if (ApplyAnimation && SelectedAnimation != null && PreviewSequences.Count > 0)
		{
			if (Playing && !StepThrough)
			{
				TimeSpan timeToAdd = DateTime.Now - LastEditorTick;
				AnimProgressSeconds += (float)(timeToAdd.TotalMilliseconds / 1000d);
				if (AnimProgressSeconds >= GetPreviewDurationSeconds())
				{
					if (Looping)
						AnimProgressSeconds -= GetPreviewDurationSeconds();
					else
						AnimProgressSeconds = GetPreviewDurationSeconds();
				}
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

				if (progressInTicks <= duration)
					break;

				progressInTicks -= duration;

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

						ApplyPose(from, to, outputParameter);
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
		Root.SetAnimPreviewEnabled(false);
	}
	private void ApplyPose(KeyframeDefinition from, KeyframeDefinition to, float parameter)
	{
		Root.SetAnimPreviewEnabled(true);
		foreach (PreviewAnimNode previewNode in Root.GetAllDescendantNodes<PreviewAnimNode>())
		{
			PoseDefinition fromPose = GetPose(from.name, previewNode.APName);
			PoseDefinition toPose = GetPose(to.name, previewNode.APName);
			Vector3 pos = LerpPosition(fromPose, toPose, parameter);
			Quaternion ori = LerpRotation(fromPose, toPose, parameter);

			previewNode.SetPose(pos, ori, Vector3.one);
		}
	}
	private void ApplyPose(KeyframeDefinition keyframe)
	{
		Root.SetAnimPreviewEnabled(true);
		foreach (PoseDefinition pose in keyframe.poses)
		{
			PreviewAnimNode previewNode = Root.GetAnimPreviewFor(pose.applyTo);
			if(previewNode != null)
			{
				Vector3 pos = Resolve(pose.position);
				Vector3 euler = Resolve(pose.rotation);

				previewNode.SetPose(pos, euler, pose.scale);
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
			Minecraft.Euler(a),
			Minecraft.Euler(b),
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

	protected override void EditorUpdate()
	{
		base.EditorUpdate();
		UpdateAnims();
	}
}
