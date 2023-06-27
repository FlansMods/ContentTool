using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAnimationType
{
    NONE, 
	BOTTOM_CLIP, 
	PISTOL_CLIP, 
	TOP_CLIP, 
	SIDE_CLIP, 
	P90, 
	SHOTGUN,
	RIFLE, 
	REVOLVER, 
	END_LOADED, 
	RIFLE_TOP, 
	BULLPUP, 
	ALT_PISTOL_CLIP, 
	GENERIC, 
	BACK_LOADED, 
	STRIKER, 
	BREAK_ACTION, 
	CUSTOM
}

public static class AnimationTypes
{
	public static string Convert(this EAnimationType anim)
	{
		switch(anim)
		{
			case EAnimationType.BOTTOM_CLIP: return "flansmod:ar_load_from_bottom";
			case EAnimationType.TOP_CLIP: return "flansmod:ar_load_from_top";
			case EAnimationType.SIDE_CLIP: return "flansmod:ar_load_from_side";
			case EAnimationType.BULLPUP: return "flansmod:ar_bullpup";
			case EAnimationType.P90: return "flansmod:p90";

			case EAnimationType.RIFLE: return "flansmod:bolt_action_rifle";
			case EAnimationType.RIFLE_TOP: return "flansmod:rifle_clip_fed_top";
			
			case EAnimationType.PISTOL_CLIP: return "flansmod:pistol";
			case EAnimationType.ALT_PISTOL_CLIP: return "flansmod:pistol";
			case EAnimationType.REVOLVER: return "flansmod:revolver";

			case EAnimationType.END_LOADED: return "flansmod:end_loaded";
			case EAnimationType.BACK_LOADED: return "flansmod:back_loaded";
			
			case EAnimationType.SHOTGUN: return "flansmod:shotgun_shell_loader";
			case EAnimationType.BREAK_ACTION: return "flansmod:shotgun_break_action";
			
			case EAnimationType.GENERIC: return "";
			case EAnimationType.STRIKER: return "";
			default: return "";
		}
	}
}
