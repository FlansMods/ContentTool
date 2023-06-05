using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBoxEntryTopLevel : GunBoxEntry
{
	public List<GunBoxEntry> childEntries;
	
	public GunBoxEntryTopLevel(string type, List<string> requiredParts)
		: base(type, requiredParts)
	{
		childEntries = new List<GunBoxEntry>();
	}

	public void addAmmo(string type, List<string> requiredParts)
	{
		childEntries.Add(new GunBoxEntry(type, requiredParts));
	}
}

public class GunBoxEntry
{
	public string type;
	public List<string> requiredParts;
	
	public GunBoxEntry(string type, List<string> requiredParts)
	{
		this.type = type;
		this.requiredParts = requiredParts;
	}
}

public class GunBoxPage
{
	public List<GunBoxEntryTopLevel> entries;
	/**
		* Points to the gun box entry we are currently reading from file. Allows for the old format to write in ammo on a separate line to the gun.
		*/
	private GunBoxEntryTopLevel currentlyEditing;
	public string name;
	
	public GunBoxPage(string s)
	{
		name = s;
		entries = new List<GunBoxEntryTopLevel>();
	}
	
	public void addNewEntry(string type, List<string> requiredParts)
	{
		GunBoxEntryTopLevel entry = new GunBoxEntryTopLevel(type, requiredParts);
		entries.Add(entry);
		currentlyEditing = entry;
	}
	
	public void addAmmoToCurrentEntry(string type, List<string> requiredParts)
	{
		currentlyEditing.addAmmo(type, requiredParts);
	}
}

public class GunBoxType : BoxType
{
    /**
	 * Stores pages for the gun box indexed by their title (unlocalized!)
	 */
	public Dictionary<string, GunBoxPage> pagesByTitle = new Dictionary<string, GunBoxPage>();
	public List<GunBoxPage> pages = new List<GunBoxPage>();
	
	/**
	 * Points to the page we are currently adding to.
	 */
	public GunBoxPage currentPage;
	
	public GunBoxPage defaultPage;
	
	private static int lastIconIndex = 2;
}
