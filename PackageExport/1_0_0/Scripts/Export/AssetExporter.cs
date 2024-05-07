using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class AssetExporter
{
	public virtual string GetAssetExtension() { return "asset"; }
	public abstract bool MatchesAssetType(System.Type type);
	public virtual bool MatchesAsset(Object asset) { return true; }
	public abstract string GetOutputFolder();
	public virtual string GetOutputExtension() { return "json"; }
	public virtual string LocationToExportPath(ResourceLocation location, bool inAssets) 
	{
		string outputPrefix = GetOutputFolder();
		string idAndPath = location.ID.StartsWith(outputPrefix) ? location.ID : $"{outputPrefix}/{location.ID}";

		return $"{FlansModExport.EXPORT_ROOT}/{(inAssets ? "assets" : "data")}/{location.Namespace}/{idAndPath}.{GetOutputExtension()}";
	}

	public abstract void Export(Object asset, bool overwrite = false, IVerificationLogger verifications = null);

	protected string GetAssetPath(Object asset)
	{
		return AssetDatabase.GetAssetPath(asset);
	}
}

public abstract class AssetCopyExporter : AssetExporter
{
	protected string GetDefaultAssetExportPath(Object asset)
	{
		if (asset.TryGetLocation(out ResourceLocation loc))
		{
			return LocationToExportPath(loc, true);
		}
		Debug.LogError($"Could not resolve location of {asset}");
		return "";
	}

	public override void Export(Object asset, bool overwrite = false, IVerificationLogger verifications = null)
	{
		try
		{
			string src = GetAssetPath(asset);
			string dst = GetDefaultAssetExportPath(asset);
			if (!overwrite && File.Exists(dst))
			{
				verifications?.Neutral($"Not exporting {dst} because it already exists");
				return;
			}
			string dstFolder = dst.Substring(0, dst.LastIndexOfAny(Utils.SLASHES));
			if (!File.Exists(src))
			{
				verifications?.Failure($"Export Failed: {src} file did not exist while exporting {asset}");
				return;
			}
			if (!Directory.Exists(dstFolder))
			{
				verifications?.Success($"Created directory {dstFolder} while exporting {asset}");
				Directory.CreateDirectory(dstFolder);
			}
			File.Copy(src, dst, true);
			verifications?.Success($"Copied asset '{asset.name}' to {dst}");
		}
		catch(System.Exception e)
		{
			verifications?.Exception(e);
		}
	}
}

public abstract class MultiAssetExporter<TEnum> : AssetExporter where TEnum : struct
{
	protected abstract bool CanExport(TEnum exportType, Object asset, out string path);
	protected abstract void ExportElement(TEnum exportType, string toPath, Object asset, bool overwrite = false, IVerificationLogger verifications = null);

	public override void Export(Object asset, bool overwrite = false, IVerificationLogger verifications = null)
	{
		foreach (TEnum exportType in System.Enum.GetValues(typeof(TEnum)))
		{
			if (CanExport(exportType, asset, out string path))
				ExportElement(exportType, path, asset, overwrite, verifications);
		}
	}
}

public abstract class AssetToJsonExporter<TExportType> : MultiAssetExporter<TExportType> where TExportType : struct
{
	protected override void ExportElement(TExportType exportType, string exportPath, Object asset, bool overwrite = false, IVerificationLogger verifications = null)
	{
		if (asset == null)
		{
			verifications?.Failure($"Null asset passed to {this} exporter");
			return;
		}

		if (!overwrite && File.Exists(exportPath))
		{
			verifications?.Neutral($"Not exporting {exportPath} because it already exists");
			return;
		}

		JObject jObject = ToJson(exportType, asset, verifications);
		ExportJsonToFile(jObject, exportPath, verifications);
	}
	protected void ExportJsonToFile(JObject json, string exportPath, IVerificationLogger verifications = null)
	{
		ExportJsonToFile(JsonReadWriteUtils.WriteFormattedJson(json), exportPath, verifications);
	}
	protected void ExportJsonToFile(string jsonString, string exportPath, IVerificationLogger verifications = null)
	{
		try
		{
			File.WriteAllText(exportPath, jsonString);
			verifications?.Success($"Exported Json to '{exportPath}'");
		}
		catch (System.Exception e)
		{
			verifications?.Exception(e, $"Failed to export Json to {exportPath}");
		}
	}
	protected abstract JObject ToJson(TExportType exportType, Object asset, IVerificationLogger verifications = null);
}

public enum EDuplicatedAssetExport
{
	TO_DATA,
	TO_ASSETS,
}
public abstract class DuplicatedJsonExporter : AssetToJsonExporter<EDuplicatedAssetExport>
{
	public virtual bool ExportToData() { return true; }
	public virtual bool ExportToAssets() { return true; }

	protected string GetAssetsExportPath(Object asset)
	{
		if (asset.TryGetLocation(out ResourceLocation loc))
		{
			return LocationToExportPath(loc, true);
		}
		Debug.LogError($"Could not resolve location of {asset}");
		return "";
	}
	protected string GetDataExportPath(Object asset)
	{
		if (asset.TryGetLocation(out ResourceLocation loc))
		{
			return LocationToExportPath(loc, true);
		}
		Debug.LogError($"Could not resolve location of {asset}");
		return "";
	}

	protected override bool CanExport(EDuplicatedAssetExport exportType, Object asset, out string path)
	{
		switch (exportType)
		{
			case EDuplicatedAssetExport.TO_DATA:
				if (ExportToData())
				{
					path = GetDataExportPath(asset);
					return true;
				}
				break;
			case EDuplicatedAssetExport.TO_ASSETS:
				if (ExportToAssets())
				{
					path = GetAssetsExportPath(asset);
					return true;
				}
				break;
		}
		path = "";
		return false;
	}
}