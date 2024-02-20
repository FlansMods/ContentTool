using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAccumulationSource
{
	PerStacks,
	PerLevel,
	PerAttachment,
	PerMagFullness,
	PerMagEmptiness,
}

public static class AccumulationSources
{
	public const int NUM_SOURCES = 5;
}