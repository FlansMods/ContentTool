using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;
using System.Collections.Generic;

[System.Serializable]
public class ActionDefinition : Element, ISerializationCallbackReceiver
{
	[JsonField]
	public EActionType actionType = EActionType.Invalid;
	[JsonField]
	[Tooltip("In seconds")]
	public float duration = 0.0f;
	[JsonField]
	public SoundDefinition[] sounds = new SoundDefinition[0];
	[JsonField]
	public string itemStack = "";
	[JsonField]
	[Tooltip("These will be applied to this action if applicable")]
	public ModifierDefinition[] modifiers = new ModifierDefinition[0];
	[JsonField]
	public string scopeOverlay = "";
	[JsonField]
	public string anim = "";


	[FormerlySerializedAs("fovFactor")]
	[HideInInspector]
	public float _fovFactor = float.NaN;

	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		if(!float.IsNaN(_fovFactor) && _fovFactor != 0.0f)
		{
			List<ModifierDefinition> mods = new List<ModifierDefinition>(modifiers);
			foreach(ModifierDefinition mod in mods)
			{
				if(mod.stat == Constants.STAT_ZOOM_FOV_FACTOR)
					return;
			}
			mods.Add(new ModifierDefinition()
			{
				stat = Constants.STAT_ZOOM_FOV_FACTOR,
				accumulators = new StatAccumulatorDefinition[] {
					new StatAccumulatorDefinition()
					{
						value = _fovFactor,
						operation = EAccumulationOperation.BaseAdd,
					}
				}
			});
			modifiers = mods.ToArray();
		}
	}
}
