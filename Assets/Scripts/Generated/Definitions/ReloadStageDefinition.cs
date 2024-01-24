using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ReloadStageDefinition
{
	[JsonField]
[Tooltip("The full duration of this reload stage, in seconds")]
	public float duration = 1.0f;
	[JsonField]
[Tooltip("All actions to run when entering this reload stage")]
	public ActionGroupDefinition actions = new ActionGroupDefinition();
}
