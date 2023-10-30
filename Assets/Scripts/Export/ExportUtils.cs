using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ExportUtils
{
	
}

public class ExportTree
{
	public UnityEngine.Object Asset = null;
	public string AssetRelativeExportPath = "";
	public bool ExportSuccessful = false;
	public List<ExportTree> Children = new List<ExportTree>();
}


public class ExportDirectory : IDisposable
{
	public string Path { get; private set; }
	public ExportDirectory(string root)
	{
		Path = root;
		if (!Directory.Exists(Path))
			Directory.CreateDirectory(Path);
	}
	public ExportDirectory Subdir(string folder)
	{
		return new ExportDirectory($"{Path}/{folder}");
	}
	public string File(string fileName)
	{
		return $"{Path}/{fileName}";
	}

	public override string ToString() { return Path; }

	public void Dispose()
	{

	}
}