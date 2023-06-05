using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TypeFile
{
    //public EnumType type;
    public string name;
    public List<string> lines;
    //public static HashMap<EnumType, ArrayList<TypeFile>> files;
    private int readerPosition = 0;

    public TypeFile(/*EnumType t,*/ string s)
    {
        //type = t;
        name = s;
        lines = new List<string>();
    }

    public string readLine()
    {
        if (readerPosition == lines.Count)
            return null;
        return lines[readerPosition++];
    }

    public void addLine(string newLine)
    {
        lines.Add(newLine);
    }
}
