using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static ElementExtensions;
using UnityEngine.UIElements;

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

	// -----------------------------------------------------------------------------------------------------------------
	#region Flanimation -> UnityAnim
	// -----------------------------------------------------------------------------------------------------------------
	public AnimatorController ConvertToUnityAnim(FlanimationDefinition flanimation, TurboRootNode referenceModel = null)
	{
		string flanimationPath = AssetDatabase.GetAssetPath(flanimation);
		string controllerPath = referenceModel != null
			? flanimationPath.Replace(".asset", $"_{referenceModel.name}_controller.asset")
			: flanimationPath.Replace(".asset", "_controller.asset");


		AnimatorController animController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
		foreach(SequenceDefinition sequence in flanimation.sequences)
		{
			string clipPath = controllerPath.Replace("controller.asset", $"{sequence.name}.asset");
			AnimationClip clip = CreateClip(flanimation, sequence, clipPath, referenceModel);
			animController.AddMotion(clip);
		}

		EditorUtility.SetDirty(animController);
		return animController;
	}
	public AnimationClip CreateClip(FlanimationDefinition flanimation, SequenceDefinition sequence, string path, TurboRootNode referenceModel = null)
	{
		AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
		if (clip == null)
		{
			clip = new AnimationClip();
			AssetDatabase.CreateAsset(clip, path);
			clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
		}
		
		List<string> framesNeeded = sequence.GetKeyframesNeeded();
		Dictionary<float, SequenceEntryDefinition> frameTimings = sequence.GetFrameTimings();
		Dictionary<string, List<ElementExtensions.EBindingType>> bindings = new Dictionary<string, List<ElementExtensions.EBindingType>>();
		foreach (KeyframeDefinition keyframe in flanimation.keyframes)
		{
			if (framesNeeded.Contains(keyframe.name))
			{
				keyframe.AppendBindingsNeeded(bindings);
			}
		}

		foreach (var kvp in bindings)
		{
			string partName = kvp.Key;

			if (referenceModel != null)
			{
				if (referenceModel.TryFindDescendant($"ap_{partName}", out AttachPointNode apNode))
				{
					EditorCurveBinding[] curveBindings = AnimationUtility.GetAnimatableBindings(apNode.gameObject, referenceModel.gameObject);
					Bind(flanimation, clip, partName, kvp.Value, curveBindings, frameTimings, apNode.transform);
				}
			}
			else
			{
				// TODO: Default anim export with no reference model chosen

			}
		}
		EditorUtility.SetDirty(clip);
		return clip;
	}
	private void Bind(FlanimationDefinition flanimation, AnimationClip clip, string partName, List<EBindingType> bindingsNeeed, EditorCurveBinding[] curveBindings, Dictionary<float, SequenceEntryDefinition> frameTimings, Transform referenceModelAP = null)
	{
		foreach (EditorCurveBinding curveBinding in curveBindings)
		{
			EBindingType bindingType = GetBindingType(curveBinding);
			if (bindingsNeeed.Contains(bindingType))
			{
				AnimationCurve curve = CreateCurveFor(flanimation, partName, bindingType, frameTimings, referenceModelAP);
				AnimationUtility.SetEditorCurve(clip, curveBinding, curve);
			}
		}
	}
	public AnimationCurve CreateCurveFor(FlanimationDefinition flanimation, string partName, EBindingType bindingType, Dictionary<float, SequenceEntryDefinition> frameTimings, Transform referenceModelAP = null)
	{
		AnimationCurve curve = new AnimationCurve();
		foreach (var kvp in frameTimings)
		{
			float time = kvp.Key;
			SequenceEntryDefinition entry = kvp.Value;
			if (flanimation.TryGetKeyframe(entry.frame, out KeyframeDefinition rootKeyframe))
			{
				// Get the pose, first by checking the keyframe, then by climbing the parent heirarchy
				PoseDefinition pose = null;
				if (rootKeyframe.TryGetPose(partName, out PoseDefinition rootPose))
					pose = rootPose;
				else
				{
					foreach (KeyframeDefinition parentKeyframe in flanimation.GetRecursiveParentsOf(rootKeyframe))
					{
						if (parentKeyframe.TryGetPose(partName, out PoseDefinition parentPose))
						{
							pose = parentPose;
							break;
						}
					}
				}

				if (pose != null)
				{
					float defaultValue =
					curve.AddKey(new Keyframe()
					{
						time = time,
						value = (float)pose.GetBindingValue(bindingType) + GetDefaultValue(referenceModelAP, bindingType),
					});
				}
			}
		}
		return curve;
	}
	private float GetDefaultValue(Transform target, EBindingType bindingType)
	{
		if (target != null)
		{
			switch (bindingType)
			{
				case EBindingType.PosX:			return target.localPosition.x;
				case EBindingType.PosY:			return target.localPosition.y;
				case EBindingType.PosZ:			return target.localPosition.z;
				case EBindingType.OriX:			return target.localRotation.x;
				case EBindingType.OriY:			return target.localRotation.y;
				case EBindingType.OriZ:			return target.localRotation.z;
				case EBindingType.OriW:			return target.localRotation.z;
				case EBindingType.ScaleX:		return target.localScale.x;
				case EBindingType.ScaleY:		return target.localScale.y;
				case EBindingType.ScaleZ:		return target.localScale.z;
			}
		}
		return 0f;
	}
	private EBindingType GetBindingType(EditorCurveBinding curveBinding)
	{
		switch (curveBinding.propertyName)
		{
			case "m_LocalPosition.x": return EBindingType.PosX;
			case "m_LocalPosition.y": return EBindingType.PosY;
			case "m_LocalPosition.z": return EBindingType.PosZ;
			case "m_LocalRotation.x": return EBindingType.OriX;
			case "m_LocalRotation.y": return EBindingType.OriY;
			case "m_LocalRotation.z": return EBindingType.OriZ;
			case "m_LocalRotation.w": return EBindingType.OriW;
			case "m_LocalScale.x": return EBindingType.ScaleX;
			case "m_LocalScale.y": return EBindingType.ScaleY;
			case "m_LocalScale.z": return EBindingType.ScaleZ;
			default: return EBindingType.None;
		}
	}
	#endregion
	// -----------------------------------------------------------------------------------------------------------------


	public void ConvertToFlanimation(AnimatorController controller)
	{
		
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
			if (sequence.name.StartsWith(controller.name+"_"))
				sequence.name = sequence.name.Substring(controller.name.Length + 1);
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
