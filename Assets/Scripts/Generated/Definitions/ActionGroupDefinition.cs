using UnityEngine;

[System.Serializable]
public class ActionGroupDefinition
{
	[JsonField]
	public string key = "default";
	[JsonField]
	public bool canActUnderwater = true;
	[JsonField]
	public bool canActUnderOtherLiquid = false;
	[JsonField]
[Tooltip("If true, attachments that add an action in the same place will override this one")]
	public bool canBeOverriden = false;
	[JsonField]
[Tooltip("If true, then this action will only work if the other hand is empty")]
	public bool twoHanded = false;
	[JsonField]
[Tooltip("Refers to gun modes like Full Auto, but applies to all actions")]
	public ERepeatMode repeatMode = ERepeatMode.SemiAuto;
	[JsonField]
	public float repeatDelay = 0.0f;
	[JsonField]
[Tooltip("Number of times to repeat the fire action if we are set to burst fire mode")]
	public int repeatCount = 0;
	[JsonField]
[Tooltip("If using minigun fire mode, this is the time (in seconds) that it will take to spin up the motor and start shooting")]
	public float spinUpDuration = 1.0f;
	[JsonField]
[Tooltip("The distance this action should be 'heard' from, in block radius. Modify this for silenced actions to not even show up in the net msgs of other players")]
	public float loudness = 150f;
	[JsonField]
	public ActionDefinition[] actions = new ActionDefinition[0];
}
