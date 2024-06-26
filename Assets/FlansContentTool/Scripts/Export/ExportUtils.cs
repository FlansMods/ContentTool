using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public interface IFileAccess
{
	bool CanExport(string path);
}

public class AllowListFileAccess : IFileAccess
{
	public List<string> AllowedPaths;
	public AllowListFileAccess(List<string> allowedPaths)
	{
		AllowedPaths = allowedPaths;
	}
	public bool CanExport(string path)
	{
		return AllowedPaths.Contains(path);
	}
}

public class CarefulFileAccess : IFileAccess
{
	public bool AllowCreateDir;
	public bool AllowOverwrite;

	public CarefulFileAccess(bool allowCreateDir, bool allowOverwrite)
	{
		AllowCreateDir = allowCreateDir;
		AllowOverwrite = allowOverwrite;
	}

	public bool CanExport(string path)
	{
		if(!AllowOverwrite)
		{
			if (File.Exists(path))
				return false;
		}
		if(!AllowCreateDir)
		{
			string folder = path.Substring(0, path.LastIndexOfAny(Utils.SLASHES));
			if (!Directory.Exists(folder))
				return false;
		}
		return true;
	}

}

public static class ExportUtils
{
	public static bool SaveAsPrefab(this IFileAccess fileAccess,
		GameObject rootGameObject, string path, bool destroyInstance = true, IVerificationLogger logger = null)
	{
		if (!fileAccess.CanExport(path))
			return false;

		try
		{
			string folderPath = path.Substring(0, path.LastIndexOfAny(Utils.SLASHES));
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			PrefabUtility.SaveAsPrefabAsset(rootGameObject, path);
			if (destroyInstance)
				UnityEngine.Object.DestroyImmediate(rootGameObject);

			return true;
		}
		catch (Exception e)
		{
			logger?.Exception(e);
			return false;
		}
	}
	public static bool SaveScriptableObject(this IFileAccess fileAccess,
		ScriptableObject scriptableObject, string path, IVerificationLogger logger = null)
	{
		if (!fileAccess.CanExport(path))
			return false;

		try
		{
			string folderPath = path.Substring(0, path.LastIndexOfAny(Utils.SLASHES));
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			AssetDatabase.CreateAsset(scriptableObject, path);

			return true;
		}
		catch (Exception e)
		{
			logger?.Exception(e);
			return false;
		}
	}

	public delegate GameObject PrefabCreateFunc();
	public static bool TryCreatePrefab(this IFileAccess fileAccess, PrefabCreateFunc createFunc,
		string path, bool destroyInstance = true, IVerificationLogger logger = null)
	{
		if (fileAccess.CanExport(path))
		{
			GameObject go = createFunc();
			return fileAccess.SaveAsPrefab(go, path, destroyInstance, logger);
		}
		return false;
	}

	public static bool TryCreateScriptableObject<T>(this IFileAccess fileAccess, string path, Action<T> initFunc, IVerificationLogger logger = null) where T : ScriptableObject
	{
		if (fileAccess.CanExport(path))
		{
			T newInstance = ScriptableObject.CreateInstance<T>();
			initFunc(newInstance);
			return fileAccess.SaveScriptableObject(newInstance, path, logger);
		}
		return false;
	}

	public static void Copy(this IFileAccess fileAccess,
		string from, string to, IVerificationLogger logger = null)
	{
		if (!fileAccess.CanExport(to))
			return;

		try
		{
			if (!File.Exists(from))
			{
				logger?.Neutral($"Failed to import {from} - FileNotFound");
				return;
			}

			string folderPath = to.Substring(0, to.LastIndexOf('/'));
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			File.Copy(from, to, true);
			logger?.Success($"Copied file from '{from}' to '{to}'");
		}
		catch (Exception e)
		{
			logger?.Exception(e);
		}
	}
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