using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
public class HitscanDefinition : Element
{
	[JsonField]
	[Tooltip("Number of raycasts to create")]
	public int shotCount = 1;
	[JsonField]
	[Tooltip("The radius within which to apply splash effects. If 0, any Impacts on splash won't trigger")]
	public float splashRadius = 0.0f;
	[JsonField]
	[Tooltip("Impact settings. You probably want at least a ShotPosition, or ShotEntity and ShotBlock")]
	public ImpactDefinition[] impacts = new ImpactDefinition[0];
	[JsonField]
	[Tooltip("How much stuff can this bullet pass through?")]
	public float penetrationPower = 0.0f;
}
