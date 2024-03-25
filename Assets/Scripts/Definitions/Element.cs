using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Element
{

}

public static class ElementExtensions
{
	public static VecWithOverride Clone(this VecWithOverride vec)
	{
		return new VecWithOverride()
		{
			xValue = vec.xValue,
			xOverride = vec.xOverride,
			yValue = vec.yValue,
			yOverride = vec.yOverride,
			zValue = vec.zValue,
			zOverride = vec.zOverride,
		};
	}
	public static PoseDefinition Clone(this PoseDefinition pose)
	{
		return new PoseDefinition()
		{
			applyTo = pose.applyTo,
			position = pose.position.Clone(),
			rotation = pose.rotation.Clone(),
			scale = pose.scale,
		};
	}
	public static KeyframeDefinition Clone(this KeyframeDefinition keyframe, string newName = null)
	{
		KeyframeDefinition copy = new KeyframeDefinition()
		{
			name = newName != null ? newName : $"{keyframe.name}_new",
			parents = new string[keyframe.parents.Length],
			poses = new PoseDefinition[keyframe.poses.Length],
		};
		for (int i = 0; i < keyframe.parents.Length; i++)
			copy.parents[i] = keyframe.parents[i];
		for (int i = 0; i < keyframe.poses.Length; i++)
			copy.poses[i] = keyframe.poses[i].Clone();
		return copy;
	}
	public static SequenceEntryDefinition Clone(this SequenceEntryDefinition entry, string newName = null)
	{
		return new SequenceEntryDefinition()
		{
			tick = entry.tick,
			entry = entry.entry,
			exit = entry.exit,
			frame = entry.frame,
		};
	}
	public static SequenceDefinition Clone(this SequenceDefinition seq, string newName = null)
	{
		SequenceDefinition copy = new SequenceDefinition()
		{
			name = newName != null ? newName : $"{seq.name}_new",
			ticks = seq.ticks,
			frames = new SequenceEntryDefinition[seq.frames.Length],
		};
		for (int i = 0; i < seq.frames.Length; i++)
			copy.frames[i] = seq.frames[i].Clone();
		return copy;
	}
	public static Dictionary<float, SequenceEntryDefinition> GetFrameTimings(this SequenceDefinition sequence)
	{
		Dictionary<float, SequenceEntryDefinition> times = new Dictionary<float, SequenceEntryDefinition>();
		foreach (SequenceEntryDefinition entry in sequence.frames)
		{
			times.Add(entry.tick / 20f, entry);
		}
		return times;
	}
	public static List<string> GetKeyframesNeeded(this FlanimationDefinition flanimation, SequenceDefinition sequence)
	{
		List<string> framesNeeded = new List<string>();
		foreach (SequenceEntryDefinition entry in sequence.frames)
		{
			if (!framesNeeded.Contains(entry.frame))
				framesNeeded.Add(entry.frame);
			if(TryGetKeyframe(flanimation, entry.frame, out KeyframeDefinition keyframe))
			{
				foreach (KeyframeDefinition parentKeyframe in GetRecursiveParentsOf(flanimation, keyframe))
					if (!framesNeeded.Contains(parentKeyframe.name))
						framesNeeded.Add(parentKeyframe.name);
			}
		}
		return framesNeeded;
	}
	public static List<string> GetKeyframesNeeded(this SequenceDefinition sequence)
	{
		List<string> framesNeeded = new List<string>();
		foreach (SequenceEntryDefinition entry in sequence.frames)
		{
			if (!framesNeeded.Contains(entry.frame))
				framesNeeded.Add(entry.frame);
		}
		return framesNeeded;
	}
	public static bool TryGetKeyframe(this FlanimationDefinition flanimation, string name, out KeyframeDefinition result)
	{
		for(int i = 0; i < flanimation.keyframes.Length; i++)
		{
			if (flanimation.keyframes[i].name == name)
			{
				result = flanimation.keyframes[i];
				return true;
			}
		}
		result = null;
		return false;
	}
	public static bool TryGetPose(this KeyframeDefinition keyframe, string applyTo, out PoseDefinition result)
	{
		for (int i = 0; i < keyframe.poses.Length; i++)
		{
			if (keyframe.poses[i].applyTo == applyTo)
			{
				result = keyframe.poses[i];
				return true;
			}
		}
		result = null;
		return false;
	}
	public static IEnumerable<KeyframeDefinition> GetRecursiveParentsOf(this FlanimationDefinition flanimation, KeyframeDefinition startingFrame)
	{
		foreach (string parentName in startingFrame.parents)
			if (TryGetKeyframe(flanimation, parentName, out KeyframeDefinition parentKeyframe))
			{
				yield return parentKeyframe;
				foreach (KeyframeDefinition parentOfParent in GetRecursiveParentsOf(flanimation, parentKeyframe))
					yield return parentOfParent;
			}
	}

	public static IEnumerable<KeyframeDefinition> GetParentsOf(this FlanimationDefinition flanimation, KeyframeDefinition startingFrame)
	{
		foreach (string parentName in startingFrame.parents)
			if (TryGetKeyframe(flanimation, parentName, out KeyframeDefinition keyframe))
				yield return keyframe;
	}
	public enum EBindingType
	{
		PosX,
		PosY,
		PosZ,
		OriX,
		OriY,
		OriZ,
		OriW,
		ScaleX,
		ScaleY,
		ScaleZ,
		None,
	}
	public static void AppendBindingsNeeded(this KeyframeDefinition keyframe, Dictionary<string, List<EBindingType>> bindingList)
	{
		foreach (PoseDefinition pose in keyframe.poses)
		{
			if (!bindingList.TryGetValue(pose.applyTo, out List<EBindingType> forThisPart))
			{
				forThisPart = new List<EBindingType>();
				bindingList[pose.applyTo] = forThisPart;
			}

			foreach(EBindingType bindingType in GetBindingsNeeded(pose))
				if(!forThisPart.Contains(bindingType))
					forThisPart.Add(bindingType);
		}
	}

	public static IEnumerable<EBindingType> GetBindingsNeeded(this PoseDefinition pose)
	{
		if(pose.position.xValue != 0.0f || pose.position.xOverride.Length > 0
		|| pose.position.yValue != 0.0f || pose.position.yOverride.Length > 0
		|| pose.position.zValue != 0.0f || pose.position.zOverride.Length > 0)
		{
			yield return EBindingType.PosX;
			yield return EBindingType.PosY;
			yield return EBindingType.PosZ;
		}
		if(pose.rotation.xValue != 0.0f || pose.rotation.xOverride.Length > 0
		|| pose.rotation.yValue != 0.0f || pose.rotation.yOverride.Length > 0
		|| pose.rotation.zValue != 0.0f || pose.rotation.zOverride.Length > 0)
		{
			yield return EBindingType.OriX;
			yield return EBindingType.OriY;
			yield return EBindingType.OriZ;
			yield return EBindingType.OriW;
		}
		if (pose.scale.x != 1.0f || pose.scale.y != 1.0f || pose.scale.z != 1.0f)
		{
			yield return EBindingType.ScaleX;
			yield return EBindingType.ScaleY;
			yield return EBindingType.ScaleZ;
		}
	}

	// TODO: Overrides
	public static double GetBindingValue(this PoseDefinition pose, EBindingType bindingType)
	{
		Quaternion quat = Quaternion.Euler((float)pose.rotation.xValue, (float)pose.rotation.yValue, (float)pose.rotation.zValue);
		switch (bindingType)
		{
			case EBindingType.PosX: return pose.position.xValue;
			case EBindingType.PosY: return pose.position.yValue;
			case EBindingType.PosZ: return pose.position.zValue;
			case EBindingType.OriX: return quat.x;
			case EBindingType.OriY: return quat.y;
			case EBindingType.OriZ: return quat.z;
			case EBindingType.OriW: return quat.w;
			case EBindingType.ScaleX: return pose.scale.x;
			case EBindingType.ScaleY: return pose.scale.y;
			case EBindingType.ScaleZ: return pose.scale.z;
			default: return 0d;
		}
	}
}
