using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : InfoType
{
	public List<string> classes = new List<string>();
	
	public int score = 0;
	
	public int teamColour = 0xffffff;
	public char textColour = 'f';
	
	public string hat = "";
	public string chest = "";
	public string legs = "";
	public string shoes = "";
}
