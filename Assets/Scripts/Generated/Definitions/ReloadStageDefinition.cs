using UnityEngine;

[System.Serializable]
public class ReloadStageDefinition
{
	[JsonField]
	public float duration = 1.0f;
	[JsonField]
	public ActionDefinition[] actions = new ActionDefinition[0];
}
