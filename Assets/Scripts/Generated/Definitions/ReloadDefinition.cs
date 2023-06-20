using UnityEngine;

[System.Serializable]
public class ReloadDefinition
{
	[JsonField]
	public bool manualReloadAllowed = true;
	[JsonField]
	public bool autoReloadWhenEmpty = true;
	[JsonField]
	public ReloadStageDefinition start = new ReloadStageDefinition();
	[JsonField]
	public ReloadStageDefinition eject = new ReloadStageDefinition();
	[JsonField]
	public ReloadStageDefinition loadOne = new ReloadStageDefinition();
	[JsonField]
	public ReloadStageDefinition end = new ReloadStageDefinition();
}
