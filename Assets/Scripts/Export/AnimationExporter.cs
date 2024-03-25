using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class UnityAnimationExporter : DuplicatedJsonExporter
{
	public static UnityAnimationExporter INST = new UnityAnimationExporter();

	private static System.Type TYPE_OF_ANIMATOR_CONTROLLER = typeof(AnimatorController);
	public override bool MatchesAssetType(System.Type type) { return TYPE_OF_ANIMATOR_CONTROLLER.IsAssignableFrom(type); }

	public override string GetOutputFolder() { return "flanimations"; }

	protected override JObject ToJson(EDuplicatedAssetExport exportType, Object asset, IVerificationLogger verifications = null)
	{
		if (asset is AnimatorController controller)
		{
			if (Convert(controller, out SequenceDefinition[] sequences, out KeyframeDefinition[] keyframes))
			{
				JToken jSequences = JsonReadWriteUtils.ExportInternal(sequences);
				JToken jKeyframes = JsonReadWriteUtils.ExportInternal(keyframes);

				return new JObject
				{
					["sequences"] = jSequences,
					["keyframes"] = jKeyframes,
				};
			}
		}

		verifications?.Failure("Could not export AnimatorController");
		return new JObject();
	}

	private class ProtoPoseDef
	{
		public Vector3 pos;
		public Quaternion ori;
		public Vector3 scale;
	}

	// Creates a FlanimationDefinition
	public bool Convert(AnimatorController controller, out SequenceDefinition[] sequences, out KeyframeDefinition[] keyframes)
    {
		List<SequenceDefinition> sequenceList = new List<SequenceDefinition>();
		List<KeyframeDefinition> keyframeList = new List<KeyframeDefinition>();
		foreach (AnimationClip clip in controller.animationClips)
		{
			SequenceDefinition sequence = new SequenceDefinition();
			sequence.name = clip.name;
			sequence.ticks = 20;

			// First pass, convert curves with keyframes into "ProtoPoseDef" structures
			Dictionary<int, Dictionary<string, ProtoPoseDef>> keyframesForThisSequence = new Dictionary<int, Dictionary<string, ProtoPoseDef>>();
			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
			{
				AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
				foreach(Keyframe key in curve.keys)
				{
					int tick = Mathf.FloorToInt(key.time*20f);
					if(!keyframesForThisSequence.TryGetValue(tick, out Dictionary<string, ProtoPoseDef> keyDef))
					{
						keyDef = new Dictionary<string, ProtoPoseDef>();
						keyframesForThisSequence[tick] = keyDef;
					}

					string applyTo = binding.path.Substring(binding.path.LastIndexOfAny(Utils.SLASHES)+1);
					if (applyTo.StartsWith("ap_"))
						applyTo = applyTo.Substring("ap_".Length);
					if(!keyDef.TryGetValue(applyTo, out ProtoPoseDef pose))
					{
						pose = new ProtoPoseDef();
						keyDef[applyTo] = pose;
					}

					CopyFromBinding(binding, key, pose);
				}
			}

			// Second pass, pack those ProtoPoseDefs up into PoseDefinitions
			List<SequenceEntryDefinition> sequenceEntries = new List<SequenceEntryDefinition>();
			foreach(var kvp in keyframesForThisSequence)
			{
				KeyframeDefinition keyframe = new KeyframeDefinition()
				{
					name = $"{clip.name}_tick_{kvp.Key}",
					parents = new string[0],
					poses = new PoseDefinition[kvp.Value.Count],
				};
				int index = 0;
				foreach(var innerKvp in kvp.Value)
				{
					Vector3 euler = innerKvp.Value.ori.eulerAngles;
					keyframe.poses[index] = new PoseDefinition()
					{
						applyTo = innerKvp.Key,
						position = new VecWithOverride()
						{
							xValue = innerKvp.Value.pos.x,
							yValue = innerKvp.Value.pos.y,
							zValue = innerKvp.Value.pos.z,
						},
						rotation = new VecWithOverride()
						{
							xValue = euler.x,
							yValue = euler.y,
							zValue = euler.z,
						},
						scale = innerKvp.Value.scale
					};
					index++;
				}
				keyframeList.Add(keyframe);

				SequenceEntryDefinition entry = new SequenceEntryDefinition()
				{
					frame = $"{clip.name}_tick_{kvp.Key}",
					tick = kvp.Key,
					entry = ESmoothSetting.linear,
					exit = ESmoothSetting.linear,
				};
				sequenceEntries.Add(entry);
			}

			sequence.frames = sequenceEntries.ToArray();
			sequenceList.Add(sequence);
		}
		sequences = sequenceList.ToArray();
		keyframes = keyframeList.ToArray();
		return true;
	}

	private void CopyFromBinding(EditorCurveBinding binding, Keyframe keyframe, ProtoPoseDef intoPose)
	{
		switch(binding.propertyName)
		{
			case "m_LocalPosition.x":	intoPose.pos.x = keyframe.value;	break;
			case "m_LocalPosition.y":	intoPose.pos.y = keyframe.value;	break;
			case "m_LocalPosition.z":	intoPose.pos.z = keyframe.value;	break;
			case "m_LocalRotation.x":	intoPose.ori.x = keyframe.value;	break;
			case "m_LocalRotation.y":	intoPose.ori.y = keyframe.value;	break;
			case "m_LocalRotation.z":	intoPose.ori.z = keyframe.value;	break;
			case "m_LocalRotation.w":	intoPose.ori.w = keyframe.value;	break;
			case "m_LocalScale.x":		intoPose.scale.x = keyframe.value;  break;
			case "m_LocalScale.y":		intoPose.scale.y = keyframe.value;  break;
			case "m_LocalScale.z":		intoPose.scale.z = keyframe.value;  break;
		}
	}
}
