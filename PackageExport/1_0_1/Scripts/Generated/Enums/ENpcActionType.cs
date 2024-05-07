using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ENpcActionType
{
	Neutral_Idle,
	Neutral_Wander,
	Neutral_LookAtPlayer,
	Neutral_LookAtMobs,
	Neutral_LookAtAnimals,

	Friendly_ShowItemForSale,
	Friendly_ChatToPlayer,

	Hostile_Retaliate,
	Hostile_TeleportAway,
	Hostile_Indifferent,
}
