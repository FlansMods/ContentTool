using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/BulletDefinition")]
public class BulletDefinition : Definition
{
	[JsonField]
	public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
	public int roundsPerItem = 1;
	[JsonField]
	[Tooltip("Any number of hitscan rays. These shoot the target instantly")]
	public HitscanDefinition[] hitscans = new HitscanDefinition[0];
	[JsonField]
	[Tooltip("Any number of entity projectiles. These take time to get to their target")]
	public ProjectileDefinition[] projectiles = new ProjectileDefinition[0];
	[JsonField]
	public AbilityDefinition[] triggers = new AbilityDefinition[0];
	[JsonField]
	[Tooltip("These action groups can be triggered at the point of impact")]
	public ActionGroupDefinition[] actionGroups = new ActionGroupDefinition[0];
}
