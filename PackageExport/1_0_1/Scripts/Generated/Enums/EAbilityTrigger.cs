using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EAbilityTrigger
{
	AlwaysOn,
	Instant,

	ShotEntity,
	ShotBlock,
	ShotAndBrokeBlock,
	ShotHeadshot,

	ReloadStart,
	ReloadEject,
	ReloadLoadOne,
	ReloadEnd,

	StartActionGroup,
	TriggerActionGroup,
	EndActionGroup,
	SwitchMode,

	AddToInventory,     // Not in yet
	RemoveFromInventory,// Not in yet
	Equip,
	Unequip,

	BulletConsumed,
	FirstBulletConsumed,
	LastBulletConsumed,

	RaycastAction,


}