using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationDefinition))]
public class AnimationEditor : Editor
{
	private List<bool> foldouts = new List<bool>();

	private static List<string> foldedOut = new List<string>();

	private bool IsFoldout(string key)
	{
		return foldedOut.Contains(key);
	}
	private void SetFoldout(string key, bool bSet)
	{
		if(bSet && !foldedOut.Contains(key))
			foldedOut.Add(key);
		if(!bSet && foldedOut.Contains(key))
			foldedOut.Remove(key);
	}


	private void SetFoldout(int i, bool bSet)
	{
		while(i >= foldouts.Count)
			foldouts.Add(false);
		foldouts[i] = bSet;
	}
	private bool IsFoldout(int i) {
		while(i >= foldouts.Count)
			foldouts.Add(false);
		return foldouts[i];
	}

	public List<string> DefinitionNames = new List<string>();

	public void RefreshAssetCache()
	{
		DefinitionNames.Clear();
		string[] defPaths = AssetDatabase.FindAssets("t:Definition");
		Debug.Log($"Found {defPaths.Length}");
		foreach(string defPath in defPaths)
		{
			Object obj = AssetDatabase.LoadMainAssetAtPath(defPath);
			if(obj is Definition def)
			{
				Debug.Log($"Found {def} at {defPath}");
				if(def != null)
				{
					DefinitionNames.Add(defPath);
				}
			}
			else
			{
				Debug.LogWarning($"Found unexpected {obj} of type {obj.GetType()} at {defPath}");
			}
		}
	}

    public override void OnInspectorGUI()
	{
		AnimationDefinition instance = (AnimationDefinition)target;
		if(instance != null)
		{
			// Acquire names for dropdowns
			List<string> sequenceNames = new List<string>(instance.sequences.Length);
			for (int i = 0; i < instance.sequences.Length; i++)
			{
				sequenceNames.Add(instance.sequences[i].name);
			}
			List<string> frameNames = new List<string>(instance.keyframes.Length);
			for (int i = 0; i < instance.keyframes.Length; i++)
			{
				frameNames.Add(instance.keyframes[i].name);
			}

			KeyframeEditor(frameNames, instance);
			SequenceEditor(frameNames, instance);
			if(GUI.changed)
			{
				EditorUtility.SetDirty(instance);
			}
		}
	}

	private void KeyframeEditor(List<string> frameNames, AnimationDefinition instance)
	{
		int num32Blocks = (frameNames.Count + 31) / 32;
		List<string>[] frameNamesInBlocksOf32 = new List<string>[num32Blocks];
		for(int p = 0; p < num32Blocks; p++)
		{
			frameNamesInBlocksOf32[p] = new List<string>();
			for(int q = 0; q < 32; q++)
			{
				if(p * 32 + q < frameNames.Count)
					frameNamesInBlocksOf32[p].Add(frameNames[p * 32 + q]);
			}
		}

		FlanStyles.BigHeader("Keyframe Editor");
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("New Keyframe"))
		{
			Undo.RecordObject(instance, instance.name);
			KeyframeDefinition[] newArray = new KeyframeDefinition[instance.keyframes.Length + 1];
			for(int i = 0; i < instance.keyframes.Length; i++)
			{
				newArray[i] = instance.keyframes[i];
			}
			newArray[instance.keyframes.Length] = new KeyframeDefinition()
			{
				name = $"keyframe_{instance.keyframes.Length}",
			};
			instance.keyframes = newArray;
		}
		if(GUILayout.Button("Swap x"))
		{
			for(int i = 0; i < instance.keyframes.Length; i++)
			{
				for(int j = 0; j < instance.keyframes[i].poses.Length; j++)
				{
					VecWithOverride pos = instance.keyframes[i].poses[j].position;
					instance.keyframes[i].poses[j].position = new VecWithOverride()
					{
						xValue = -pos.xValue,
						xOverride = pos.xOverride,
						yValue = pos.yValue,
						yOverride = pos.yOverride,
						zValue = pos.zValue,
						zOverride = pos.zOverride,
					};
				}
			}
		}
		GUILayout.EndHorizontal();


		int keyframeToDelete = -1;
		for(int i = 0; i < instance.keyframes.Length; i++)
		{
			string keyframe_id = $"{instance.name}_{i}";
			bool bFoldout = EditorGUILayout.Foldout(IsFoldout(keyframe_id), instance.keyframes[i].name);
			SetFoldout(keyframe_id, bFoldout);

			if(bFoldout)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(16));
				GUILayout.BeginVertical();
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Keyframe:");
					instance.keyframes[i].name = GUILayout.TextField(instance.keyframes[i].name);
					if(GUILayout.Button("Delete Keyframe"))
						keyframeToDelete = i;
					GUILayout.EndHorizontal();

					// Parent selector
					GUILayout.BeginHorizontal();
					List<string> parents = new List<string>(instance.keyframes[i].parents);
					List<string> newParents = new List<string>(frameNames.Count);
					for(int block = 0; block < num32Blocks; block++)
					{
						int numInBlock = frameNamesInBlocksOf32[block].Count;
						int mask = 0;
						for(int n = 0; n < numInBlock; n++)
						{
							if(parents.Contains(frameNamesInBlocksOf32[block][n]))
								mask |= 1 << n;
						}
						if(block == 0)
							mask = EditorGUILayout.MaskField($"Parents ({parents.Count})", mask, frameNamesInBlocksOf32[block].ToArray());
						else
							mask = EditorGUILayout.MaskField(mask, frameNamesInBlocksOf32[block].ToArray());
						for(int n = 0; n < numInBlock; n++)
						{
							if(((mask >> n) & 0x1) != 0)
							{
								newParents.Add(frameNamesInBlocksOf32[block][n]);
							}
						}
					}
					instance.keyframes[i].parents = newParents.ToArray();
					GUILayout.EndHorizontal();

					int poseToDelete = -1;

					// Headers
					int pxPerCol = 48;
					GUILayout.BeginHorizontal();
					GUILayout.Label("Pose");
					GUILayout.Label("x", GUILayout.Width(pxPerCol));
					GUILayout.Label("y", GUILayout.Width(pxPerCol));
					GUILayout.Label("z", GUILayout.Width(pxPerCol));
					GUILayout.Label("roll", GUILayout.Width(pxPerCol));
					GUILayout.Label("yaw", GUILayout.Width(pxPerCol));
					GUILayout.Label("pitch", GUILayout.Width(pxPerCol));
					if(GUILayout.Button("Add", GUILayout.Width(64)))
					{
						Undo.RecordObject(instance, instance.name);
						PoseDefinition[] poses = new PoseDefinition[instance.keyframes[i].poses.Length + 1];
						for(int j = 0; j < instance.keyframes[i].poses.Length; j++)
						{
							poses[j] = instance.keyframes[i].poses[j];
						}
						poses[instance.keyframes[i].poses.Length] = new PoseDefinition();
						poses[instance.keyframes[i].poses.Length].applyTo = "body";
						instance.keyframes[i].poses = poses;
					}
					GUILayout.EndHorizontal();


					for(int j = 0; j < instance.keyframes[i].poses.Length; j++)
					{
						PoseDefinition poseDef = instance.keyframes[i].poses[j];

						// First row
						GUILayout.BeginHorizontal();
						poseDef.applyTo = GUILayout.TextField(poseDef.applyTo);
						if(poseDef.position.xOverride.Length == 0)
							poseDef.position.xValue = EditorGUILayout.DoubleField(poseDef.position.xValue, GUILayout.Width(pxPerCol));
						else
							poseDef.position.xOverride = GUILayout.TextField(poseDef.position.xOverride, GUILayout.Width(pxPerCol));
						
						if(poseDef.position.yOverride.Length == 0)
							poseDef.position.yValue = EditorGUILayout.DoubleField(poseDef.position.yValue, GUILayout.Width(pxPerCol));
						else
							poseDef.position.yOverride = GUILayout.TextField(poseDef.position.yOverride, GUILayout.Width(pxPerCol));
						
						if(poseDef.position.zOverride.Length == 0)
							poseDef.position.zValue = EditorGUILayout.DoubleField(poseDef.position.zValue, GUILayout.Width(pxPerCol));
						else
							poseDef.position.zOverride = GUILayout.TextField(poseDef.position.zOverride, GUILayout.Width(pxPerCol));
						
						if(poseDef.rotation.xOverride.Length == 0)
							poseDef.rotation.xValue = EditorGUILayout.DoubleField(poseDef.rotation.xValue, GUILayout.Width(pxPerCol));
						else
							poseDef.rotation.xOverride = GUILayout.TextField(poseDef.rotation.xOverride, GUILayout.Width(pxPerCol));
						
						if(poseDef.rotation.yOverride.Length == 0)
							poseDef.rotation.yValue = EditorGUILayout.DoubleField(poseDef.rotation.yValue, GUILayout.Width(pxPerCol));
						else
							poseDef.rotation.yOverride = GUILayout.TextField(poseDef.rotation.yOverride, GUILayout.Width(pxPerCol));
						
						if(poseDef.rotation.zOverride.Length == 0)
							poseDef.rotation.zValue = EditorGUILayout.DoubleField(poseDef.rotation.zValue, GUILayout.Width(pxPerCol));
						else
							poseDef.rotation.zOverride = GUILayout.TextField(poseDef.rotation.zOverride, GUILayout.Width(pxPerCol));


						if(GUILayout.Button("Delete", GUILayout.Width(64)))
						{
							poseToDelete = j;
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label("");
						poseDef.position.xOverride = GUILayout.Toggle(poseDef.position.xOverride.Length > 0, "", GUILayout.Width(pxPerCol)) ? (poseDef.position.xOverride.Length == 0 ? "param" : poseDef.position.xOverride) : "";
						poseDef.position.yOverride = GUILayout.Toggle(poseDef.position.yOverride.Length > 0, "", GUILayout.Width(pxPerCol)) ? (poseDef.position.yOverride.Length == 0 ? "param" : poseDef.position.yOverride) : "";
						poseDef.position.zOverride = GUILayout.Toggle(poseDef.position.zOverride.Length > 0, "", GUILayout.Width(pxPerCol)) ? (poseDef.position.zOverride.Length == 0 ? "param" : poseDef.position.zOverride) : "";
						poseDef.rotation.xOverride = GUILayout.Toggle(poseDef.rotation.xOverride.Length > 0, "", GUILayout.Width(pxPerCol)) ? (poseDef.rotation.xOverride.Length == 0 ? "param" : poseDef.rotation.xOverride) : "";
						poseDef.rotation.yOverride = GUILayout.Toggle(poseDef.rotation.yOverride.Length > 0, "", GUILayout.Width(pxPerCol)) ? (poseDef.rotation.yOverride.Length == 0 ? "param" : poseDef.rotation.yOverride) : "";
						poseDef.rotation.zOverride = GUILayout.Toggle(poseDef.rotation.zOverride.Length > 0, "", GUILayout.Width(pxPerCol)) ? (poseDef.rotation.zOverride.Length == 0 ? "param" : poseDef.rotation.zOverride) : "";
						GUILayout.Label("", GUILayout.Width(64));
						GUILayout.EndHorizontal();
					}

					if(poseToDelete != -1)
					{
						Undo.RecordObject(instance, instance.name);
						PoseDefinition[] poses = new PoseDefinition[instance.keyframes[i].poses.Length - 1];
						for(int j = 0; j < instance.keyframes[i].poses.Length - 1; j++)
						{
							poses[j] = j < poseToDelete ? instance.keyframes[i].poses[j] : instance.keyframes[i].poses[j-1];
						}
						instance.keyframes[i].poses = poses;
					}
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		if(keyframeToDelete != -1)
		{
			Undo.RecordObject(instance, instance.name);
			KeyframeDefinition[] newArray = new KeyframeDefinition[instance.keyframes.Length - 1];
			for(int i = 0; i < instance.keyframes.Length - 1; i++)
			{
				newArray[i] = i >= keyframeToDelete ? instance.keyframes[i+1] : instance.keyframes[i];
			}
			instance.keyframes = newArray;
		}
	}

	private void SequenceEditor(List<string> frameNames, AnimationDefinition instance)
	{
		FlanStyles.BigHeader("Animation Sequence Editor");
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("New Sequence"))
		{
			Undo.RecordObject(instance, instance.name);
			SequenceDefinition[] newArray = new SequenceDefinition[instance.sequences.Length + 1];
			for(int i = 0; i < instance.sequences.Length; i++)
			{
				newArray[i] = instance.sequences[i];
			}
			newArray[instance.sequences.Length] = new SequenceDefinition()
			{
				name = "sequence"
			};
			instance.sequences = newArray;
		}
		GUILayout.EndHorizontal();
		int sequenceToDelete = -1;
		for(int i = 0; i < instance.sequences.Length; i++)
		{
			string sequence_id = $"{instance.name}_s_{i}";
			bool bFoldout = EditorGUILayout.Foldout(IsFoldout(sequence_id), instance.sequences[i].name);
			SetFoldout(sequence_id, bFoldout);

			if(bFoldout)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(16));
				GUILayout.BeginVertical();
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Sequence:");
					instance.sequences[i].name = GUILayout.TextField(instance.sequences[i].name, GUILayout.MinWidth(128));
					if(GUILayout.Button("Delete Sequence"))
						sequenceToDelete = i;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Frame");
					GUILayout.Label("Blend-in", GUILayout.Width(64));
					GUILayout.Label("Blend-out", GUILayout.Width(64));
					GUILayout.Label("Tick", GUILayout.Width(32));
					if(GUILayout.Button("Add", GUILayout.Width(48)))
					{
						Undo.RecordObject(instance, instance.name);
						SequenceEntryDefinition[] frames = new SequenceEntryDefinition[instance.sequences[i].frames.Length + 1];
						for(int j = 0; j < instance.sequences[i].frames.Length; j++)
						{
							frames[j] = instance.sequences[i].frames[j];
						}
						frames[instance.sequences[i].frames.Length] = new SequenceEntryDefinition();
						frames[instance.sequences[i].frames.Length].frame = "idle";
						instance.sequences[i].frames = frames;
					}
					GUILayout.EndHorizontal();
					int frameToDelete = -1;

					for(int j = 0; j < instance.sequences[i].frames.Length; j++)
					{
						SequenceEntryDefinition frameDef = instance.sequences[i].frames[j];
						string frame_id = $"{sequence_id}_{j}";

						GUILayout.BeginHorizontal();
						// Frame select
						int frameIndex = frameNames.IndexOf(frameDef.frame);
						int newIndex = EditorGUILayout.Popup(frameIndex, frameNames.ToArray());
						if(newIndex != -1)
						{
							frameDef.frame = frameNames[newIndex];
						}
						EditorGUI.BeginDisabledGroup(j == 0);
						frameDef.entry = (ESmoothSetting)EditorGUILayout.EnumPopup(frameDef.entry, GUILayout.Width(64));
						EditorGUI.EndDisabledGroup();
						EditorGUI.BeginDisabledGroup(j == instance.sequences[i].frames.Length - 1);
						frameDef.exit = (ESmoothSetting)EditorGUILayout.EnumPopup(frameDef.exit, GUILayout.Width(64));
						EditorGUI.EndDisabledGroup();
						frameDef.tick = EditorGUILayout.IntField(frameDef.tick, GUILayout.Width(32));
						if(GUILayout.Button("Delete", GUILayout.Width(48)))
						{
							frameToDelete = j;
						}
						GUILayout.EndHorizontal();
					}

					if(frameToDelete != -1)
					{
						Undo.RecordObject(instance, instance.name);
						SequenceEntryDefinition[] frames = new SequenceEntryDefinition[instance.sequences[i].frames.Length - 1];
						for(int j = 0; j < instance.sequences[i].frames.Length - 1; j++)
						{
							frames[j] = j < frameToDelete ? instance.sequences[i].frames[j] : instance.sequences[i].frames[j+1];
						}
						instance.sequences[i].frames = frames;
					}

				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		if(sequenceToDelete != -1)
		{
			Undo.RecordObject(instance, instance.name);
			SequenceDefinition[] newArray = new SequenceDefinition[instance.sequences.Length - 1];
			for(int i = 0; i < instance.sequences.Length - 1; i++)
			{
				newArray[i] = i >= sequenceToDelete ? instance.sequences[i+1] : instance.sequences[i];
			}
			instance.sequences = newArray;
		}

		for(int i = 0; i < instance.sequences.Length; i++)
		{
			instance.sequences[i].ticks = 0;
			for(int j = 0; j < instance.sequences[i].frames.Length; j++)
			{
				instance.sequences[i].ticks = Mathf.Max(instance.sequences[i].frames[j].tick, instance.sequences[i].ticks);
			}
		}
	}

	private void VecWithOverrideField(VecWithOverride vec, string xLabel, string yLabel, string zLabel)
	{
		GUILayout.BeginVertical();
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(xLabel, GUILayout.MinWidth(32));
				vec.xValue = EditorGUILayout.DoubleField(vec.xValue, GUILayout.MaxWidth(64));
				bool bHasOverride = vec.xOverride.Length > 0;
					GUILayout.Label($"Override:");
				bool bTickbox = EditorGUILayout.Toggle(bHasOverride, GUILayout.ExpandWidth(false));
				if(bTickbox && !bHasOverride)
					vec.xOverride = "param_name";
				if(!bTickbox && bHasOverride)
					vec.xOverride = "";

				EditorGUI.BeginDisabledGroup(vec.xOverride.Length == 0);
				vec.xOverride = GUILayout.TextField(vec.xOverride, GUILayout.Width(256));
				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(yLabel, GUILayout.MinWidth(32));
				vec.yValue = EditorGUILayout.DoubleField(vec.yValue, GUILayout.MaxWidth(64));
				bool bHasOverride = vec.yOverride.Length > 0;
				GUILayout.Label($"Override:");
				bool bTickbox = EditorGUILayout.Toggle(bHasOverride, GUILayout.ExpandWidth(false));
				if(bTickbox && !bHasOverride)
					vec.yOverride = "param_name";
				if(!bTickbox && bHasOverride)
					vec.yOverride = "";

				EditorGUI.BeginDisabledGroup(vec.yOverride.Length == 0);
				vec.yOverride = GUILayout.TextField(vec.yOverride, GUILayout.Width(256));
				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(zLabel, GUILayout.MinWidth(32));
				vec.zValue = EditorGUILayout.DoubleField(vec.zValue, GUILayout.MaxWidth(64));
				bool bHasOverride = vec.zOverride.Length > 0;
					GUILayout.Label($"Override:");
				bool bTickbox = EditorGUILayout.Toggle(bHasOverride, GUILayout.ExpandWidth(false));
				if(bTickbox && !bHasOverride)
					vec.zOverride = "param_name";
				if(!bTickbox && bHasOverride)
					vec.zOverride = "";

				EditorGUI.BeginDisabledGroup(vec.zOverride.Length == 0);
				vec.zOverride = GUILayout.TextField(vec.zOverride, GUILayout.Width(256));
				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();			
		}
		GUILayout.EndVertical();



	}
}
