using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const string STAT_SHOT_SPREAD = "spread";
	public const string STAT_SHOT_VERTICAL_RECOIL = "vertical_recoil";
	public const string STAT_SHOT_HORIZONTAL_RECOIL = "horizontal_recoil";
	public const string STAT_SHOT_SPEED = "speed";
	public const string STAT_SHOT_BULLET_COUNT = "bullet_count";
	public const string STAT_SHOT_PENETRATION_POWER = "penetration_power";
	public const string STAT_SHOT_SPREAD_PATTERN = "spread_pattern";
	public const string STAT_IMPACT_DAMAGE = "impact_damage";
	public const string STAT_IMPACT_POTION_EFFECT_ON_TARGET = "potion_effect_on_target";
	public const string STAT_IMPACT_KNOCKBACK = "knockback";
	public const string STAT_IMPACT_MULTIPLIER_VS_PLAYERS = "multiplier_vs_players";
	public const string STAT_IMPACT_MULTIPLIER_VS_VEHICLES = "multiplier_vs_vehicles";
	public const string STAT_IMPACT_SPLASH_DAMAGE_RADIUS = "splash_damage_radius";
	public const string STAT_IMPACT_SPLASH_DAMAGE_FALLOFF = "splash_damage_falloff";
	public const string STAT_IMPACT_POTION_EFFECT_ON_SPLASH = "potion_effect_on_splash";
	public const string STAT_IMPACT_SET_FIRE_TO_TARGET = "set_fire_to_target";
	public const string STAT_IMPACT_FIRE_SPREAD_RADIUS = "fire_spread_radius";
	public const string STAT_IMPACT_FIRE_SPREAD_AMOUNT = "fire_spread_amount";
	public static readonly string[] STAT_SUGGESTIONS = new string[]
	{
		STAT_SHOT_SPREAD,
		STAT_SHOT_VERTICAL_RECOIL,
		STAT_SHOT_HORIZONTAL_RECOIL,
		STAT_SHOT_SPEED,
		STAT_SHOT_BULLET_COUNT,
		STAT_SHOT_PENETRATION_POWER,
		STAT_SHOT_SPREAD_PATTERN,
		STAT_IMPACT_DAMAGE,
		STAT_IMPACT_POTION_EFFECT_ON_TARGET,
		STAT_IMPACT_KNOCKBACK,
		STAT_IMPACT_MULTIPLIER_VS_PLAYERS,
		STAT_IMPACT_MULTIPLIER_VS_VEHICLES,
		STAT_IMPACT_SPLASH_DAMAGE_RADIUS,
		STAT_IMPACT_SPLASH_DAMAGE_FALLOFF,
		STAT_IMPACT_POTION_EFFECT_ON_SPLASH,
		STAT_IMPACT_SET_FIRE_TO_TARGET,
		STAT_IMPACT_FIRE_SPREAD_RADIUS,
		STAT_IMPACT_FIRE_SPREAD_AMOUNT,
	};
}
