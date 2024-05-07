using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundExporter : AssetCopyExporter
{
	public static SoundExporter INST = new SoundExporter();

	public override string GetAssetExtension() { return "ogg"; }
	public override string GetOutputExtension() { return "ogg"; }
	private static System.Type TYPE_OF_AUDIO = typeof(AudioClip);
	public override bool MatchesAssetType(System.Type type) { return TYPE_OF_AUDIO.IsAssignableFrom(type); }
	public override string GetOutputFolder() { return "sounds"; }

}
