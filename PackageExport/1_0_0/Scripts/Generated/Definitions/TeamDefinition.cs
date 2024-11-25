using UnityEngine;
using static ResourceLocation;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "Flans Mod/TeamDefinition")]
public class TeamDefinition : Definition
{
	[JsonField]
	public string[] classes = new string[0];
	[JsonField]
	public ColourDefinition flagColour = new ColourDefinition();
	[JsonField]
	public char textColour = 'f';
	[JsonField]
	public ItemStackDefinition hat = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition chest = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition legs = new ItemStackDefinition();
	[JsonField]
	public ItemStackDefinition shoes = new ItemStackDefinition();
}
