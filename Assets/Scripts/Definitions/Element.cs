using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

}
