using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class ShotDefinition : Element
{
	[JsonField]
	public float verticalRecoil = 3.0f;
	[JsonField]
	public float horizontalRecoil = 0.0f;
	[JsonField]
	public float spread = 0.0f;
	[JsonField]
	public ESpreadPattern spreadPattern = ESpreadPattern.FilledCircle;
	[JsonField]
	public float speed = 0.0f;
	[JsonField]
	[Tooltip("Number of raycasts or bullet entities to create")]
	public int bulletCount = 1;
	[JsonField]
	[Tooltip("If using minigun fire mode, this is the max rotational speed (in degrees/second) of the barrels")]
	public float spinSpeed = 360.0f;
	[JsonField]
	public string[] breaksBlockTags = new string[0];
	[JsonField]
	public float penetrationPower = 1.0f;
	[JsonField]
	public ImpactDefinition impact = new ImpactDefinition();
	[JsonField]
	public bool hitscan = true;
	[JsonField]
	[Tooltip("If set to a non-zero amount, this projectile will have a fuse timer, in seconds")]
	public float fuseTime = 0.0f;
	[JsonField]
	public float gravityFactor = 1.0f;
	[JsonField]
	public bool sticky = false;
	[JsonField]
	[Tooltip("How quickly a projectile rotates to face the direction of travel")]
	public float turnRate = 0.5f;
	[JsonField]
	[Tooltip("Percent speed loss per tick (1/20s)")]
	public float dragInAir = 0.01f;
	[JsonField]
	public string trailParticles = "";
	[JsonField]
	public float secondsBetweenTrailParticles = 0.25f;
	[JsonField]
	[Tooltip("Percent speed loss per tick (1/20s)")]
	public float dragInWater = 0.2f;
	[JsonField]
	public string waterParticles = "";
}
