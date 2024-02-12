using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
	public const string STAT_GROUP_REPEAT_MODE = "repeat_mode";
	public const string STAT_GROUP_REPEAT_DELAY = "repeat_delay";
	public const string STAT_GROUP_REPEAT_COUNT = "repeat_count";
	public const string STAT_GROUP_SPIN_UP_DURATION = "spin_up_duration";
	public const string STAT_GROUP_LOUDNESS = "loudness";


	public const string STAT_SHOT_SPREAD = "spread";
	public const string STAT_SHOT_VERTICAL_RECOIL = "vertical_recoil";
	public const string STAT_SHOT_HORIZONTAL_RECOIL = "horizontal_recoil";
	public const string STAT_SHOT_SPEED = "speed";
	public const string STAT_SHOT_BULLET_COUNT = "bullet_count";
	public const string STAT_SHOT_PENETRATION_POWER = "penetration_power";
	public const string STAT_SHOT_SPREAD_PATTERN = "spread_pattern";

	public const string STAT_IMPACT_DAMAGE = "impact_damage";
	public const string STAT_INSTANT_DAMAGE = "instant_damage";
	public const string STAT_IMPACT_POTION_EFFECT_ON_TARGET = "potion_effect_on_target";
	public const string STAT_IMPACT_KNOCKBACK = "knockback";
	public const string STAT_IMPACT_MULTIPLIER_VS_PLAYERS = "multiplier_vs_players";
	public const string STAT_IMPACT_MULTIPLIER_VS_VEHICLES = "multiplier_vs_vehicles";
	public const string STAT_IMPACT_SPLASH_DAMAGE = "splash_damage";
	public const string STAT_IMPACT_SPLASH_DAMAGE_RADIUS = "splash_damage_radius";
	public const string STAT_IMPACT_SPLASH_DAMAGE_FALLOFF = "splash_damage_falloff";
	public const string STAT_IMPACT_POTION_EFFECT_ON_SPLASH = "potion_effect_on_splash";
	public const string STAT_IMPACT_SET_FIRE_TO_TARGET = "set_fire_to_target";
	public const string STAT_IMPACT_FIRE_SPREAD_RADIUS = "fire_spread_radius";
	public const string STAT_IMPACT_FIRE_SPREAD_AMOUNT = "fire_spread_amount";
	public const string STAT_IMPACT_EXPLOSION_RADIUS = "explosion_radius";

	public const string STAT_MELEE_DAMAGE = "melee_damage";
	public const string STAT_TOOL_REACH = "reach";
	public const string STAT_TOOL_HARVEST_LEVEL = "tool_level";
	public const string STAT_TOOL_HARVEST_SPEED = "harvest_speed";
	public const string STAT_ZOOM_FOV_FACTOR = "fov_factor";
	public const string STAT_ZOOM_SCOPE_OVERLAY = "scope_overlay";
	public const string STAT_ANIM = "anim";
	public const string STAT_BLOCK_ID = "block_id";
	public const string STAT_DURATION = "duration";
	public const string STAT_HEAL_AMOUNT = "heal_amount";
	public const string STAT_FEED_AMOUNT = "feed_amount";
	public const string STAT_FEED_SATURATION = "feed_saturation";
	public const string STAT_SOUND_PITCH = "pitch";
	public const string STAT_LIGHT_STRENGTH = "flashlight_strength";
	public const string STAT_LIGHT_RANGE = "flashlight_range";
	public const string STAT_EYE_LINE_ROLL = "eye_line_roll";

	public const string KEY_ENTITY_TAG = "entity_tag";
	public const string KEY_ENTITY_ID = "entity_id";
	public const string KEY_ACTION_KEY = "action_key";
	public const string KEY_MODEL_ID = "model_id";
	public const string KEY_MODE = "mode";
	public const string KEY_SET_VALUE = "set_value";
	public const string KEY_EYE_LINE_NAME = "eye_line_name";
	public const string KEY_MOB_EFFECT_ID = "mob_effect_id";
	public const string STAT_POTION_MULTIPLIER = "potion_multiplier";
	public const string STAT_POTION_DURATION = "potion_duration";
	public const string STAT_ATTRIBUTE_MULTIPLIER = "attribute_multiplier";


	public const string STAT_LASER_ORIGIN = "laser_origin";
	public const string STAT_LASER_RED = "laser_red";
	public const string STAT_LASER_GREEN = "laser_green";
	public const string STAT_LASER_BLUE = "laser_blue";
	public const string MODAL_FIXED_LASER_DIRECTION = "fixed_laser_direction";

	public static readonly string[] STAT_SUGGESTIONS = new string[]
	{
		STAT_GROUP_REPEAT_MODE,
		STAT_GROUP_REPEAT_DELAY,
		STAT_GROUP_REPEAT_COUNT,
		STAT_GROUP_SPIN_UP_DURATION,
		STAT_GROUP_LOUDNESS,
		
		
		STAT_SHOT_SPREAD,
		STAT_SHOT_VERTICAL_RECOIL,
		STAT_SHOT_HORIZONTAL_RECOIL,
		STAT_SHOT_SPEED,
		STAT_SHOT_BULLET_COUNT,
		STAT_SHOT_PENETRATION_POWER ,
		STAT_SHOT_SPREAD_PATTERN,
		
		STAT_IMPACT_DAMAGE,
		STAT_INSTANT_DAMAGE,
		STAT_IMPACT_POTION_EFFECT_ON_TARGET,
		STAT_IMPACT_KNOCKBACK,
		STAT_IMPACT_MULTIPLIER_VS_PLAYERS,
		STAT_IMPACT_MULTIPLIER_VS_VEHICLES,
		STAT_IMPACT_SPLASH_DAMAGE,
		STAT_IMPACT_SPLASH_DAMAGE_RADIUS,
		STAT_IMPACT_SPLASH_DAMAGE_FALLOFF,
		STAT_IMPACT_POTION_EFFECT_ON_SPLASH,
		STAT_IMPACT_SET_FIRE_TO_TARGET,
		STAT_IMPACT_FIRE_SPREAD_RADIUS,
		STAT_IMPACT_FIRE_SPREAD_AMOUNT,
		STAT_IMPACT_EXPLOSION_RADIUS,
		
		STAT_MELEE_DAMAGE,
		STAT_TOOL_REACH,
		STAT_TOOL_HARVEST_LEVEL,
		STAT_TOOL_HARVEST_SPEED,
		STAT_ZOOM_FOV_FACTOR,
		STAT_ZOOM_SCOPE_OVERLAY ,
		STAT_ANIM,
		STAT_BLOCK_ID,
		STAT_DURATION,
		STAT_HEAL_AMOUNT,
		STAT_FEED_AMOUNT,
		STAT_FEED_SATURATION,
		STAT_SOUND_PITCH,
		STAT_LIGHT_STRENGTH,
		STAT_LIGHT_RANGE,
		STAT_EYE_LINE_ROLL,
		
		KEY_ENTITY_TAG,
		KEY_ENTITY_ID,
		KEY_ACTION_KEY,
		KEY_MODEL_ID,
		KEY_MODE,
		KEY_SET_VALUE,
		KEY_EYE_LINE_NAME,
		KEY_MOB_EFFECT_ID,
		STAT_POTION_MULTIPLIER,
		STAT_POTION_DURATION,
		STAT_ATTRIBUTE_MULTIPLIER,
		
		
		STAT_LASER_ORIGIN,
		STAT_LASER_RED,
		STAT_LASER_GREEN,
		STAT_LASER_BLUE,
		MODAL_FIXED_LASER_DIRECTION,
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
	};
}
