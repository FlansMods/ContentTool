using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EActionType
{
	Invalid,

	PlaySound,
	Animation,
	Shoot,
	AimDownSights,
	Scope,
	Shield, // == Block?
	Drop,
	Melee,
	Interact,
	BreakBlock,
	PlaceBlock,
	SelectBlock,
	CollectFluid,
	PlaceFluid,

	Pickaxe,
	Shovel,
	Axe,
	Hoe,
	Strip,
	Shear,
	Flatten,
}
