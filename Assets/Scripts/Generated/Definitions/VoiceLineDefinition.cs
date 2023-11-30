using UnityEngine;

[System.Serializable]
public class VoiceLineDefinition
{
	[JsonField]
	public EVoiceLineType type = EVoiceLineType.Chat;
	[JsonField]
	public string unlocalisedString = "npc.unknown.chat";
	[JsonField]
	public SoundDefinition audioClip = new SoundDefinition();
}
