using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CustomEditor(typeof(SoundEventList))]
public class SoundEventListEditor : Editor
{
	private List<string> foldoutKeys = new List<string>();

	public override void OnInspectorGUI()
	{
		SoundEventList list = (SoundEventList)target;
		if (list == null)
			return;

		int indexToDuplicate = -1;
		int indexToDelete = -1;
		for (int i = 0; i < list.SoundEvents.Count; i++)
		{
			SoundEventEntry entry = list.SoundEvents[i];
			GUILayout.BeginHorizontal();
			bool isFoldout = foldoutKeys.Contains(entry.Key);
			bool changedFoldout = EditorGUILayout.Foldout(isFoldout, entry.Key);
			if (isFoldout && !changedFoldout)
				foldoutKeys.Remove(entry.Key);
			else if (changedFoldout && !isFoldout)
				foldoutKeys.Add(entry.Key);

			GUILayout.FlexibleSpace();
			if (GUILayout.Button(FlanStyles.DuplicateEntry, GUILayout.Width(32)))
				indexToDuplicate = i;
			if (GUILayout.Button(FlanStyles.DeleteEntry, GUILayout.Width(32)))
				indexToDelete = i;
			GUILayout.EndHorizontal();

			if (changedFoldout)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(GUIContent.none, GUILayout.Width(15), GUILayout.ExpandHeight(true));
				GUILayout.BeginVertical();
				
				SoundEventNode(entry);

				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		if (GUILayout.Button(FlanStyles.AddEntry, GUILayout.Width(32)))
		{
			list.SoundEvents.Add(new SoundEventEntry());
		}

		if (indexToDelete != -1)
			list.SoundEvents.RemoveAt(indexToDelete);
		if (indexToDuplicate != -1)
		{
			SoundEventEntry dupe = new SoundEventEntry()
			{
				Key = list.SoundEvents[indexToDuplicate].Key,
				Category = list.SoundEvents[indexToDuplicate].Category,
				SoundLocations = new List<ResourceLocation>(),
			};
			foreach (ResourceLocation resLoc in list.SoundEvents[indexToDuplicate].SoundLocations)
				dupe.SoundLocations.Add(new ResourceLocation(resLoc.Namespace, resLoc.ID));

			list.SoundEvents.Insert(indexToDuplicate + 1, dupe);
		}
	}

	private void SoundEventNode(SoundEventEntry entry)
	{
		string changedKey = EditorGUILayout.DelayedTextField("Key:", entry.Key);
		if (changedKey != entry.Key)
		{
			foldoutKeys.Remove(entry.Key);
			foldoutKeys.Add(changedKey);
			entry.Key = changedKey;
			EditorUtility.SetDirty(target);
		}

		string changedCategory = EditorGUILayout.DelayedTextField("Category:", entry.Category);
		if (changedCategory != entry.Category)
		{
			entry.Category = changedCategory;
			EditorUtility.SetDirty(target);
		}

		int indexToDuplicate = -1;
		int indexToDelete = -1;
		for (int i = 0; i < entry.SoundLocations.Count; i++)
		{
			ResourceLocation resLoc = entry.SoundLocations[i];
			GUILayout.BeginHorizontal();
			bool isFoldout = foldoutKeys.Contains($"{entry.Key}/{i}");
			bool changedFoldout = EditorGUILayout.Foldout(isFoldout, $"{i}");
			if (isFoldout && !changedFoldout)
				foldoutKeys.Remove($"{entry.Key}/{i}");
			else if (changedFoldout && !isFoldout)
				foldoutKeys.Add($"{entry.Key}/{i}");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(FlanStyles.DuplicateEntry, GUILayout.Width(32)))
				indexToDuplicate = i;
			if (GUILayout.Button(FlanStyles.DeleteEntry, GUILayout.Width(32)))
				indexToDelete = i;
			GUILayout.EndHorizontal();

			if(changedFoldout)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(GUIContent.none, GUILayout.Width(15), GUILayout.ExpandHeight(true));
				GUILayout.BeginVertical();

				ResourceLocation changedLoc = ResourceLocation.EditorObjectField(resLoc, resLoc.Load<AudioClip>());
				if(changedLoc != resLoc)
				{
					entry.SoundLocations[i] = changedLoc;
					EditorUtility.SetDirty(target);
				}

				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		if (GUILayout.Button(FlanStyles.AddEntry, GUILayout.Width(32)))
		{
			entry.SoundLocations.Add(new ResourceLocation());
		}

		if (indexToDelete != -1)
			entry.SoundLocations.RemoveAt(indexToDelete);
		if(indexToDuplicate != -1)
		{
			entry.SoundLocations.Insert(
				indexToDuplicate + 1,
				new ResourceLocation(
					entry.SoundLocations[indexToDuplicate].Namespace,
					entry.SoundLocations[indexToDuplicate].ID));
		}
	}
}
