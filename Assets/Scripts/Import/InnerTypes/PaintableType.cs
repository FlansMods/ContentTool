using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPaintjobRarity
{
	UNKNOWN,
	COMMON,
	UNCOMMON,
	RARE,
	LEGENDARY,
}

public class PaintableType : InfoType
{
    //Paintjobs
    /** The list of all available paintjobs for this gun */
    public List<Paintjob> paintjobs = new List<Paintjob>();
    /** The default paintjob for this gun. This is created automatically in the load process from existing info */
    public Paintjob defaultPaintjob;
	/** Assigns IDs to paintjobs */
    public int nextPaintjobID = 1;
}

[System.Serializable]
public class Paintjob
{
	public string iconName;
    public string textureName;
    public int ID;

    public Paintjob(int id, string iconName, string textureName)
    {
        this.ID = id;
        this.iconName = iconName;
        this.textureName = textureName;
    }
}