using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class DefinitionExporter : DuplicatedJsonExporter
{

	public static DefinitionExporter DEFAULT = new DefinitionExporter("", typeof(Definition));

	public static DefinitionExporter ARMOURS = new DefinitionExporter("armours", typeof(ArmourDefinition));
	public static DefinitionExporter ATTACHMENTS = new DefinitionExporter("attachments", typeof(AttachmentDefinition));
	public static DefinitionExporter BULLETS = new DefinitionExporter("bullets", typeof(BulletDefinition));
	public static DefinitionExporter LOADOUTS = new DefinitionExporter("loadouts", typeof(LoadoutDefinition));
	public static DefinitionExporter TRAITS = new DefinitionExporter("traits", typeof(CraftingTraitDefinition));
	public static DefinitionExporter FLANIMATIONS = new DefinitionExporter("flanimations", typeof(FlanimationDefinition));
	public static DefinitionExporter GRENADES = new DefinitionExporter("grenades", typeof(GrenadeDefinition));
	public static DefinitionExporter GUNS = new DefinitionExporter("guns", typeof(GunDefinition));
	public static DefinitionExporter LOADOUT_POOLS = new DefinitionExporter("loadouts", typeof(LoadoutPoolDefinition));
	public static DefinitionExporter MAGAZINES = new DefinitionExporter("magazines", typeof(MagazineDefinition));
	public static DefinitionExporter MATERIALS = new DefinitionExporter("materials", typeof(MaterialDefinition));
	public static DefinitionExporter NPCS = new DefinitionExporter("npcs", typeof(NpcDefinition));
	public static DefinitionExporter PARTS = new DefinitionExporter("parts", typeof(PartDefinition));
	public static DefinitionExporter REWARD_BOXES = new DefinitionExporter("reward_boxes", typeof(RewardBoxDefinition));
	public static DefinitionExporter TEAMS = new DefinitionExporter("teams", typeof(TeamDefinition));
	public static DefinitionExporter TOOLS = new DefinitionExporter("tools", typeof(ToolDefinition));
	public static DefinitionExporter VEHICLES = new DefinitionExporter("vehicles", typeof(VehicleDefinition));
	public static DefinitionExporter WORKBENCHES = new DefinitionExporter("workbenches", typeof(WorkbenchDefinition));
	public static DefinitionExporter CONTROL_SCHEMES = new DefinitionExporter("control_schemes", typeof(ControlSchemeDefinition));


	private System.Type TYPE_TO_MATCH;
	public override bool MatchesAssetType(System.Type type) { return TYPE_TO_MATCH.IsAssignableFrom(type); }

	public override string GetOutputFolder() { return OutputFolder; }
	private string OutputFolder;

	public DefinitionExporter(string folderName, System.Type type)
	{
		TYPE_TO_MATCH = type;
		OutputFolder = folderName;
	}

	protected override JObject ToJson(EDuplicatedAssetExport exportType, Object asset, IVerificationLogger verifications = null)
	{
		if (JsonReadWriteUtils.GetOrCreateWriter(asset.GetType()).Write(asset) is JObject jObject)
		{
			return jObject;
		}

		verifications?.Failure("Could not export definition");
		return new JObject();
	}
}

public static class JsonReadWriteUtils
{
	public static JToken ExportInternal(object input)
	{
		if (input == null)
			return new JObject();
		return GetOrCreateWriter(input.GetType()).Write(input);
	}

	public static string WriteFormattedJson(JObject jObject)
	{
		using (System.IO.StringWriter stringWriter = new System.IO.StringWriter())
		using (JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.Formatting = Formatting.Indented;
			jsonWriter.Indentation = 4;
			jObject.WriteTo(jsonWriter);
			return stringWriter.ToString();
		}
	}

	private static Dictionary<System.Type, Writer> Writers = null;
	public static bool CheckInit()
	{
		if (Writers == null)
		{
			Writers = new Dictionary<System.Type, Writer>();

			Writers.Add(typeof(int), new IntWriter());
			Writers.Add(typeof(float), new FloatWriter());
			Writers.Add(typeof(double), new DoubleWriter());
			Writers.Add(typeof(short), new ShortWriter());
			Writers.Add(typeof(long), new LongWriter());
			Writers.Add(typeof(byte), new ByteWriter());
			Writers.Add(typeof(string), new StringWriter());
			Writers.Add(typeof(Vector3), new Vec3Writer());
			Writers.Add(typeof(bool), new BoolWriter());
			Writers.Add(typeof(VecWithOverride), new VecWithOverrideWriter());
			Writers.Add(typeof(ResourceLocation), new ResourceLocationWriter());

			Writers.Add(typeof(int[]), new ArrayWriter() { ElementWriter = Writers[typeof(int)] });
			Writers.Add(typeof(float[]), new ArrayWriter() { ElementWriter = Writers[typeof(float)] });
			Writers.Add(typeof(double[]), new ArrayWriter() { ElementWriter = Writers[typeof(double)] });
			Writers.Add(typeof(short[]), new ArrayWriter() { ElementWriter = Writers[typeof(short)] });
			Writers.Add(typeof(long[]), new ArrayWriter() { ElementWriter = Writers[typeof(long)] });
			Writers.Add(typeof(byte[]), new ArrayWriter() { ElementWriter = Writers[typeof(byte)] });
			Writers.Add(typeof(string[]), new ArrayWriter() { ElementWriter = Writers[typeof(string)] });
			Writers.Add(typeof(Vector3[]), new ArrayWriter() { ElementWriter = Writers[typeof(Vector3)] });
			Writers.Add(typeof(bool[]), new ArrayWriter() { ElementWriter = Writers[typeof(bool)] });
			Writers.Add(typeof(VecWithOverride[]), new ArrayWriter() { ElementWriter = Writers[typeof(VecWithOverride)] });
			Writers.Add(typeof(ResourceLocation[]), new ArrayWriter() { ElementWriter = Writers[typeof(ResourceLocation)] });
		}

		return true;
	}

	public static Writer GetOrCreateWriter(System.Type type)
	{
		CheckInit();

		if (Writers.TryGetValue(type, out Writer writer))
			return writer;

		if (type.IsArray)
		{
			Writer elementWriter = GetOrCreateWriter(type.GetElementType());
			writer = new ArrayWriter() { ElementWriter = elementWriter };
		}
		else if (type.IsEnum)
		{
			writer = new EnumWriter(type);
		}
		else
		{
			writer = new ClassWriter(type);
		}
		Writers.Add(type, writer);

		return writer;
	}

	public abstract class Writer 
	{
		public Dictionary<string, Writer> FieldsToWrite = new Dictionary<string, Writer>();
		public string Key;
		public FieldInfo Field;
		public abstract JToken Write(object input);
	}

	public abstract class Writer<T> : Writer
	{
		public override JToken Write(object input) { return Write((T)input); }
		public abstract JToken Write(T obj);
	}

	public class FloatWriter : Writer<float> { public override JToken Write(float f) { return f; } }
	public class DoubleWriter : Writer<double> { public override JToken Write(double d) { return d; } }
	public class ByteWriter : Writer<byte> { public override JToken Write(byte i) { return i; } }
	public class ShortWriter : Writer<short> { public override JToken Write(short i) { return i; } }
	public class IntWriter : Writer<int> { public override JToken Write(int i) { return i; } }
	public class LongWriter : Writer<long> { public override JToken Write(long l) { return l; } }
	public class StringWriter : Writer<string> { public override JToken Write(string s) { return s; } }
	public class Vec3Writer : Writer<Vector3> { public override JToken Write(Vector3 v) { return new JArray(v.x, v.y, v.z); } }
	public class BoolWriter : Writer<bool> { public override JToken Write(bool b) { return b; } }
	public class VecWithOverrideWriter : Writer<VecWithOverride> {
		public override JToken Write(VecWithOverride obj)
		{
			return new JArray(obj.xOverride.Length > 0 ? obj.xOverride : obj.xValue.ToString(),
								obj.yOverride.Length > 0 ? obj.yOverride : obj.yValue.ToString(),
								obj.zOverride.Length > 0 ? obj.zOverride : obj.zValue.ToString()
			);
		}
	}
	public class ResourceLocationWriter : Writer<ResourceLocation> {
		public override JToken Write(ResourceLocation obj)
		{
			return obj.ToString();
		}
	}

	public class EnumWriter : Writer
	{
		public System.Type enumType;
		public EnumWriter(System.Type eType) { enumType = eType; }
		public override JToken Write(object input) 
		{ 
			return System.Enum.GetName(enumType, input);
		}
	}

	public class ArrayWriter : Writer
	{
		public Writer ElementWriter;
		public override JToken Write(object input) 
		{ 
			JArray jArray = new JArray();
			foreach(object entry in (IEnumerable)input)
			{
				jArray.Add(ElementWriter.Write(entry));
			}

			return jArray;
		}
	}

	public class ClassWriter : Writer
	{
		private Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();

		public ClassWriter(System.Type t)
		{
			foreach(FieldInfo field in t.GetFields())
			{
				JsonFieldAttribute jsonAttrib = field.GetCustomAttribute<JsonFieldAttribute>();
				if(jsonAttrib != null)
				{
					fields.Add(field.Name, field);
				}
			}
		}

		public override JToken Write(object input) 
		{ 
			JObject jObj = new JObject();
			foreach(var kvp in fields)
			{
				JToken token = ExportInternal(kvp.Value.GetValue(input));
				if(token != null)
					jObj.Add(kvp.Key, token);
			}
			return jObj;
		}
	}
}
