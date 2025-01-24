using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/TurretBlockDefinition")]
public class TurretBlockDefinition : Definition
{
	[JsonField]
    public ItemDefinition itemSettings = new ItemDefinition();
	[JsonField]
    public BlockDefinition blockSettings = new BlockDefinition();
	[JsonField]
	[Tooltip("If none are set, this will be a static turret")]
    public TurretMotionSettingsDefinition[] motionSettings = new TurretMotionSettingsDefinition[0];
	[JsonField]
    public TurretSideDefinition defaultSideSettings = new TurretSideDefinition();
	[JsonField]
    public TurretSideDefinition[] overrideSideSettings = new TurretSideDefinition[0];
	[JsonField]
	[Tooltip("If there is no slot, you should set up an embeddedGun")]
    public bool hasGunSlot = false;
	[JsonField]
    public ResourceLocation embeddedGun = InvalidLocation;
	[JsonField]
	[Tooltip("If hasGunSlot is set to true, this specifies which guns are allowed")]
    public ItemCollectionDefinition allowedGuns = new ItemCollectionDefinition();
	[JsonField]
    public ItemHoldingDefinition ammoSlots = new ItemHoldingDefinition();
}
