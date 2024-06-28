using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ContentPackPrefabGallery : MonoBehaviour
{
	public ContentPack Target;
	public Vector3 Separation = new Vector3(10, 0, 0);
	public List<ENewDefinitionType> Types = new List<ENewDefinitionType>();

	private Dictionary<RootNode, RootNode> SpawnedInstances = new Dictionary<RootNode, RootNode>();

	protected void OnEnable()
	{
		EditorApplication.update += EditorUpdate;
	}
	protected void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
	}
	protected virtual void EditorUpdate()
	{
		if(Target != null)
		{
			List<RootNode> allModels = new List<RootNode>(Target.AllModels);
			foreach(RootNode modelNode in allModels)
			{
				if (!(modelNode is TurboRootNode))
					continue;
				if(!SpawnedInstances.ContainsKey(modelNode))
				{
					// Step 1: See if we have already spawned one 
					GameObject[] matchingPrefabs = PrefabUtility.FindAllInstancesOfPrefab(modelNode.gameObject);
					if (matchingPrefabs.Length > 0)
					{
						// Only allow these if they are our child objects
						for(int i = 0; i < matchingPrefabs.Length; i++)
						{
							if(matchingPrefabs[i].transform.IsChildOf(transform) && matchingPrefabs[i].TryGetComponent(out RootNode prefabRootNode))
							{
								SpawnedInstances.Add(modelNode, prefabRootNode);
							}
						}
					}

					// Step 2: Guess we don't have it, spawn a new one
					if(!SpawnedInstances.ContainsKey(modelNode))
					{
						if(PrefabUtility.InstantiatePrefab(modelNode) is RootNode instancedRootNode)
						{
							SpawnedInstances.Add(modelNode, instancedRootNode);
						}
					}
				}
			}

			bool needReshuffle = false;
			foreach (var kvp in SpawnedInstances)
			{
				RootNode instance = kvp.Value; 
				if (instance != null)
				{
					if (instance.transform.parent != transform)
					{
						instance.transform.SetParent(transform);
						needReshuffle = true;
					}
				}
			}

			if(needReshuffle)
			{
				for(int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).localPosition = Separation * i;
					transform.GetChild(i).localRotation = Quaternion.identity;
					transform.GetChild(i).localScale = Vector3.one;
				}
			}
		}
	}
}
