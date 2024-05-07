
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public static class SpreadsheetImportExport
{
	public static string CsvExportFolderForPack(string packName) { return $"{ContentManager.ASSET_ROOT}/{packName}/csv/"; }
	public static string CsvExportPath(string packName, ENewDefinitionType defType) { return $"{ContentManager.ASSET_ROOT}/{packName}/csv/{defType}.csv"; }

	public static bool CanImportCSV(ContentPack pack, ENewDefinitionType defType)
	{
		return File.Exists(CsvExportPath(pack.name, defType));
	}

	public static void ImportFromCSV(ContentPack pack)
	{
		foreach (ENewDefinitionType defType in ExportableTypes)
			ImportFromCSV(pack, defType);
	}

	public static void ImportFromCSV(ContentPack pack, ENewDefinitionType defType)
	{
		string path = CsvExportPath(pack.name, defType);
		if (File.Exists(path))
		{
			var contentList = pack.GetSortedContent()[defType];
			if (contentList != null)
			{
				switch (defType)
				{
					case ENewDefinitionType.gun:
						Spreadsheet guns = new Spreadsheet(3);
						guns.ReadFromCSV(File.ReadAllText(path));
						foreach (GunDefinition def in pack.GetContent<GunDefinition>())
						{
							Spreadsheet.ValueReader reader = new Spreadsheet.ValueReader(guns);
							reader.Read(def);
							EditorUtility.SetDirty(def);
						}
						break;
					case ENewDefinitionType.bullet:
						Spreadsheet bullets = new Spreadsheet(3);
						bullets.ReadFromCSV(File.ReadAllText(path));
						foreach (BulletDefinition def in pack.GetContent<BulletDefinition>())
						{
							Spreadsheet.ValueReader reader = new Spreadsheet.ValueReader(bullets);
							reader.Read(def);
							EditorUtility.SetDirty(def);
						}
						break;
					case ENewDefinitionType.attachment:
						Spreadsheet attachments = new Spreadsheet(3);
						attachments.ReadFromCSV(File.ReadAllText(path));
						foreach (AttachmentDefinition def in pack.GetContent<AttachmentDefinition>())
						{
							Spreadsheet.ValueReader reader = new Spreadsheet.ValueReader(attachments);
							reader.Read(def);
							EditorUtility.SetDirty(def);
						}
						break;
				}
			}
			else Debug.LogError($"Failed to get list of {defType} content from pack {pack}");
		}
	}

	public static void ExportToCSV(ContentPack pack)
	{
		foreach (ENewDefinitionType defType in ExportableTypes)
			ExportToCSV(pack, defType);
	}
	public static void ExportToCSV(ContentPack pack, ENewDefinitionType defType)
	{
		string csvFolder = CsvExportFolderForPack(pack.name);
		if (!Directory.Exists(csvFolder))
			Directory.CreateDirectory(csvFolder);

		switch (defType)
		{
			case ENewDefinitionType.gun:
				// "gun_asset_name"/"action_group_key"/actionIndex
				Spreadsheet guns = new Spreadsheet(3);              
				foreach (GunDefinition def in pack.GetContent<GunDefinition>())
				{
					Spreadsheet.ValueWriter writer = new Spreadsheet.ValueWriter(guns);
					writer.Write(def);
				}
				ExportSheet(guns, new string[] { "gun_name", "action_group", "index" }, CsvExportPath(pack.name, defType));
				break;
			case ENewDefinitionType.attachment:
				// "attachment_name"/"action_group_override_key"/actionIndex
				Spreadsheet attachments = new Spreadsheet(3);
				foreach (AttachmentDefinition def in pack.GetContent<AttachmentDefinition>())
				{
					Spreadsheet.ValueWriter writer = new Spreadsheet.ValueWriter(attachments);
					writer.Write(def);
				}
				ExportSheet(attachments, new string[] { "attachment_name", "action_group", "index" }, CsvExportPath(pack.name, defType));
				break;
			case ENewDefinitionType.bullet:
				// "bullet_asset_name"/"action_group_key"/actionIndex
				Spreadsheet bullets = new Spreadsheet(3);
				foreach (BulletDefinition def in pack.GetContent<BulletDefinition>())
				{
					Spreadsheet.ValueWriter writer = new Spreadsheet.ValueWriter(bullets);
					writer.Write(def);
				}
				ExportSheet(bullets, new string[] { "attachment_name", "action_group", "index" }, CsvExportPath(pack.name, defType));
				break;
		}
	}
	private static void ExportSheet(Spreadsheet sheet, string[] headers, string path)
	{
		string output = sheet.ExportToCSV(headers);
		File.WriteAllText(path, output);
		Debug.Log($"Exported balance CSV at {path}");
	}

	public static ENewDefinitionType[] ExportableTypes = new ENewDefinitionType[] {
		ENewDefinitionType.bullet,
		ENewDefinitionType.attachment,
		ENewDefinitionType.gun
	};
}