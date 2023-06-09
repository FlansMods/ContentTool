using UnityEngine;

[System.Serializable]
public class ShotDefinition
{
	[JsonField]
	public float verticalRecoil = 3.0f;
	[JsonField]
	public float horizontalRecoil = 0.0f;
	[JsonField]
	public float spread = 3.0f;
	[JsonField]
	public ESpreadPattern spreadPattern = ESpreadPattern.FilledCircle;
	[JsonField]
	public bool hitscan = true;
	[JsonField]
	public float speed = 0.0f;
	[JsonField]
	public int bulletCount = 1;
	[JsonField]
	public float spinSpeed = 360.0f;
	[JsonField]
	public string[] breaksMaterials = new string[0];
	[JsonField]
	public string[] matchAmmoNames = new string[0];
	[JsonField]
	public string[] matchAmmoTags = new string[0];
	[JsonField]
	public float penetrationPower = 1.0f;
	[JsonField]
	public string trailParticles = "";
	[JsonField]
	public ImpactDefinition impact = new ImpactDefinition();
}
