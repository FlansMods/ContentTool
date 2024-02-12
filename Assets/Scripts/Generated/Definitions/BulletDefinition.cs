using UnityEngine;
using static ResourceLocation;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/BulletDefinition")]
public class BulletDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public int roundsPerItem = 1;
	[JsonField]
	public ShotDefinition shootStats = new ShotDefinition();
	[JsonField]
	public AbilityDefinition[] triggers = new AbilityDefinition[0];
	[JsonField]
[Tooltip("These action groups can be triggered at the point of impact")]
	public ActionGroupDefinition[] actionGroups = new ActionGroupDefinition[0];
}
