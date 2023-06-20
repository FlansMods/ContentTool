using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/BulletDefinition")]
public class BulletDefinition : Definition
{
	[JsonField]
	public float gravityFactor = 0.25f;
	[JsonField]
	public int maxStackSize = 64;
	[JsonField]
	public int roundsPerItem = 1;
	[JsonField]
	public ShotDefinition shootStats = new ShotDefinition();
	[JsonField]
	public string[] tags = new string[0];
	[JsonField]
	public ActionDefinition[] onShootActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] onClipEmptyActions = new ActionDefinition[0];
	[JsonField]
	public ActionDefinition[] onReloadActions = new ActionDefinition[0];
}
