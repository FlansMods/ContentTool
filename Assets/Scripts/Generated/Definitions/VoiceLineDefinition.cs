using UnityEngine;
using static ResourceLocation;

[System.Serializable]
public class VoiceLineDefinition : Element
{
	[JsonField]
	public EVoiceLineType type = EVoiceLineType.Chat;
	[JsonField]
	public string unlocalisedString = "npc.unknown.chat";
	[JsonField]
	public SoundDefinition audioClip = new SoundDefinition();
}
