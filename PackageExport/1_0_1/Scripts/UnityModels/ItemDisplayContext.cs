using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemDisplayContext
{
	NONE,
	THIRD_PERSON_LEFT_HAND,
	THIRD_PERSON_RIGHT_HAND,
	FIRST_PERSON_LEFT_HAND,
	FIRST_PERSON_RIGHT_HAND,
	HEAD,
	GUI,
	GROUND,
	FIXED,
}

public static class ItemDisplayContexts
{
	public static string GetOutputKey(this ItemDisplayContext type)
	{
		switch (type)
		{
			case ItemDisplayContext.THIRD_PERSON_RIGHT_HAND: return "thirdperson_righthand";
			case ItemDisplayContext.THIRD_PERSON_LEFT_HAND: return "thirdperson_lefthand";
			case ItemDisplayContext.FIRST_PERSON_RIGHT_HAND: return "firstperson_righthand";
			case ItemDisplayContext.FIRST_PERSON_LEFT_HAND: return "firstperson_lefthand";
			case ItemDisplayContext.HEAD: return "head";
			case ItemDisplayContext.GUI: return "gui";
			case ItemDisplayContext.GROUND: return "ground";
			case ItemDisplayContext.FIXED: return "fixed";
			default: return null;
		}
	}
}