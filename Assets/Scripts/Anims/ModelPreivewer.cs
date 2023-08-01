using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ModelPreivewer : MonoBehaviour
{
	public ModelSectionPreviewer SectionPreviewPrefab;
	public List<ModelSectionPreviewer> SectionPreviews = new List<ModelSectionPreviewer>();
	public Material material;
	public Definition Def;
	public AnimationDefinition Anim;
	private DateTime AnimStartTime = DateTime.Now;
	public string Skin = "";

	public bool Playing = false;
	public string PreviewFrame = "";
	public List<string> PreviewSequences = new List<string>();

	private string RendereredModel = "";
	public string DebugOutput = "";

	public void SetDefinition(Definition def)
	{
		Def = def;
		if(Def != null && Def.Model != null)
		{
			SetModel(Def.Model);
			SetTexture(Def.Skin);
		}
		else
		{
			SetModel(null);
		}
	}

	public void SetSkin(string skinName)
	{
		Skin = skinName;
		if(material != null)
		{
			if(skinName == Def.Skin.name)
			{
				SetTexture(Def.Skin);
				return;
			}
			foreach(Definition.AdditionalTexture tex in Def.AdditionalTextures)
			{
				if(tex.name.ToLower() == skinName.ToLower())
				{
					SetTexture(tex.texture);
					return;
				}
			}
		}

		foreach(Definition.AdditionalTexture tex in Def.AdditionalTextures)
		{
			Debug.LogWarning($"It wasn't {tex.name}");
		}
		Debug.LogError("Failed to set " + skinName + " on " + name);
	}

	public void SetModel(Model model)
	{
		foreach(ModelSectionPreviewer sectionPreview in SectionPreviews)
			if(sectionPreview != null)
				DestroyImmediate(sectionPreview.gameObject);
		SectionPreviews.Clear();

		if(model != null)
		{
			foreach(Model.Section section in model.sections)
			{
				ModelSectionPreviewer preview = Instantiate(SectionPreviewPrefab);
				preview.transform.SetParent(transform);
				preview.transform.localPosition = Vector3.zero;
				preview.transform.localRotation = Quaternion.identity;
				preview.transform.localScale = Vector3.one;
				preview.SetSection(section, model);
				SectionPreviews.Add(preview);
			}

			ModelSectionPreviewer bodyPreview = FindSectionPreview("body");
			if(bodyPreview != null)
			{
				foreach(ModelSectionPreviewer preview in SectionPreviews)
				{
					if(preview.PartName != "body")
						preview.transform.SetParent(bodyPreview.transform);
				}
			}
		}
	}

	public void SetTexture(Texture2D texture)
	{
		material = new Material(material);
		material.name = texture.name;
		material.SetTexture("_MainTex", texture);
		foreach(MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
		{
			mr.sharedMaterial = material;		
		}
	}

	public void SetAnimation(AnimationDefinition anim)
	{
		Anim = anim;
	}

	public void SetSequence(string sequenceName)
	{
		PreviewFrame = "";
		PreviewSequences.Add(sequenceName);
	}

	public void SetFrame(string frameName)
	{
		PreviewFrame = frameName;
		PreviewSequences.Clear();
	}

	public void OnEnable()
	{
		EditorApplication.update += UpdateAnims;
	}

	public void OnDisable()
	{
		EditorApplication.update -= UpdateAnims;
	}

	public void UpdateAnims()
	{
		if(Def != null && Def.Model != null && Def.Model.name != RendereredModel)
		{
			SetModel(Def.Model);
			SetTexture(Def.Skin);
			RendereredModel = Def.Model.name;
		}

		if(Playing)
		{
			if(PreviewFrame.Length > 0)
			{
				KeyframeDefinition keyframe = FindKeyframe(PreviewFrame);
				if(keyframe != null)
				{
					ApplyPose(keyframe);
				}
			}
			else if(PreviewSequences.Count > 0)
			{
				SequenceDefinition sequence = null;
				int sequenceIndex = -1;
				TimeSpan timeSinceStart = DateTime.Now - AnimStartTime;
				float progress = 20f * (float)(timeSinceStart.TotalMilliseconds / 1000d);

				for(int i = 0; i < PreviewSequences.Count; i++)
				{
					sequence = FindSequence(PreviewSequences[i], out sequenceIndex);
					if(progress < GetSequenceLength(sequence))
						break;

					progress -= GetSequenceLength(sequence);
					if(i == PreviewSequences.Count - 1)
					{
						AnimStartTime = DateTime.Now;
					}
				}

				if(sequence != null)
				{
					SequenceEntryDefinition[] segment = GetSegment(sequence, progress);
					float segmentDuration = segment[1].tick - segment[0].tick;

					// If it is valid, let's animate it
					if(segmentDuration > 0.0f)
					{
						KeyframeDefinition from = FindKeyframe(segment[0].frame);
						KeyframeDefinition to = FindKeyframe(segment[1].frame);
						if (from != null && to != null)
						{
							float linearParameter = (progress - segment[0].tick) / segmentDuration;
							linearParameter = Mathf.Clamp(linearParameter, 0f, 1f);
							float outputParameter = linearParameter;

							// Instant transitions take priority first
							if(segment[0].exit == ESmoothSetting.instant)
								outputParameter = 1.0f;
							if(segment[1].entry == ESmoothSetting.instant)
								outputParameter = 0.0f;

							// Then apply smoothing?
							if(segment[0].exit == ESmoothSetting.smooth)
							{
								// Smoothstep function
								if(linearParameter < 0.5f)
									outputParameter = linearParameter * linearParameter * (3f - 2f * linearParameter);
							}
							if(segment[1].entry == ESmoothSetting.smooth)
							{
								// Smoothstep function
								if(linearParameter > 0.5f)
									outputParameter = linearParameter * linearParameter * (3f - 2f * linearParameter);
							}

							
							foreach(var sectionPreview in SectionPreviews)
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

	private PoseDefinition GetPose(string keyframeName, string part)
	{
		KeyframeDefinition keyframe = FindKeyframe(keyframeName);
		if(keyframe != null)
		{
			foreach(PoseDefinition pose in keyframe.poses)
			{
				if(pose.applyTo == part)
					return pose;
			}

			foreach(string parent in keyframe.parents)
			{
				PoseDefinition poseFromParent = GetPose(parent, part);
				if(poseFromParent != null)
					return poseFromParent;
			}
		}
		else
		{
			Debug.LogError($"Could not find keyframe {keyframeName}");
		}

		return null;
	}

	private string GetGenericFrame(string key, out int index)
	{
		index = -1;
		int last_ = key.LastIndexOf('_');
		if(last_ != -1 && int.TryParse(key.Substring(last_ + 1), out index))
		{
			key = $"{key.Substring(0, last_)}_#";
		}
		return key;
	}

	private SequenceDefinition FindSequence(string key, out int index)
	{
		string searchFor = key;
		index = -1;
		/*int last_ = key.LastIndexOf('_');
		if(last_ != -1 && int.TryParse(key.Substring(last_ + 1), out index))
		{
			searchFor = $"{key.Substring(0, last_)}_#";
		}*/
		foreach(SequenceDefinition sequence in Anim.sequences)
		{
			if(sequence.name == searchFor)
				return sequence;
		}
		return null;
	}

	private float GetSequenceLength(SequenceDefinition sequenceDefinition)
	{
		if(sequenceDefinition == null)
			return 0f;
		int highestTick = 0;
		foreach(SequenceEntryDefinition entry in sequenceDefinition.frames)
		{
			if(entry.tick > highestTick)
				highestTick = entry.tick;
		}
		return highestTick;
	}

	private SequenceEntryDefinition[] GetSegment(SequenceDefinition sequence, float tickPlusPartial)
	{
		SequenceEntryDefinition[] entries = new SequenceEntryDefinition[2];
		entries[0] = sequence.frames[0];
		entries[1] = sequence.frames[sequence.frames.Length - 1];

		for(int i = 0; i < sequence.frames.Length; i++)
		{
			// If this is the closest above or below our current time, set it
			if(sequence.frames[i].tick <= tickPlusPartial && sequence.frames[i].tick > entries[0].tick)
				entries[0] = sequence.frames[i];

			if(sequence.frames[i].tick > tickPlusPartial && sequence.frames[i].tick < entries[1].tick)
				entries[1] = sequence.frames[i];
		}

		return entries;
	}

	private KeyframeDefinition FindKeyframe(string key)
	{
		foreach(KeyframeDefinition keyframe in Anim.keyframes)
			if(keyframe.name == key)
				return keyframe;
		return null;
	}

	private ModelSectionPreviewer FindSectionPreview(string key)
	{
		foreach(ModelSectionPreviewer section in SectionPreviews)
			if(section.PartName == key)
				return section;
		return null;
	}

	private void ApplyPose(KeyframeDefinition keyframe)
	{
		foreach(PoseDefinition pose in keyframe.poses)
		{
			ModelSectionPreviewer sectionPreview = FindSectionPreview(pose.applyTo);
			if(sectionPreview != null)
			{
				Vector3 pos = Resolve(pose.position);
				Vector3 euler = Resolve(pose.rotation);
				sectionPreview.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
				sectionPreview.transform.localEulerAngles = euler;
				sectionPreview.transform.localScale = pose.scale;
			}
		}
	}

	private Vector3 Resolve(VecWithOverride v)
	{
		return new Vector3((float)v.xValue, (float)v.yValue, (float)v.zValue);
	}

	public void OnDrawGizmos()
	{
		if(Def != null && Def.Model != null)
		{
			foreach(Model.AttachPoint ap in Def.Model.attachPoints)
			{
				Vector3 direction = Vector3.right;
				Gizmos.color = Color.white;
				switch(ap.name)
				{
					case "barrel": 
						direction = Vector3.right; 
						Gizmos.color = Color.red;
						break;
					case "grip": 
						direction = Vector3.down; 
						Gizmos.color = Color.blue;
						break;
					case "stock": 
						direction = Vector3.left; 
						Gizmos.color = Color.green;
						break;
					case "scope": 
						direction = Vector3.up; 
						Gizmos.color = Color.magenta;
						break;
				}
				Gizmos.DrawRay(ap.position, direction);
				Gizmos.DrawWireCube(ap.position + direction, Vector3.one * 0.25f);
			}

			foreach(Model.AnimationParameter animParam in Def.Model.animations)
			{
				if(animParam.isVec3)
				{
					Gizmos.DrawWireSphere(animParam.vec3Value, 0.5f);
				}
			}
		}

		if(Def is GunDefinition gunDef)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(new Vector3(-10f, 5f, 0f), 1.0f);
			Gizmos.DrawRay(new Vector3(-10f, 5f, 0f), new Vector3(30f, 0f, 0f));
		}
	}
}
