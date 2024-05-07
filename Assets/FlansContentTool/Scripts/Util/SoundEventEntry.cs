using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEventEntry 
{
    public string Key = "";
    public string Category = "player";
    public List<ResourceLocation> SoundLocations = new List<ResourceLocation>();
}
