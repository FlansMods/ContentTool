using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class JsonExporter
{
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

/*
	public class ArrayWriter<T> : Writer<T[]>
	{
		public Writer ElementWriter;
		public override JToken Write(T[] input) 
		{ 
			JArray jArray = new JArray();
			
			for(int i = 0; i < input.Length; i++)
			{
				jArray.Add(ElementWriter.Write(input[i]));
			}

			return jArray;
		}
	}*/

	public class EnumWriter : Writer
	{
		public Type enumType;
		public EnumWriter(Type eType) { enumType = eType; }
		public override JToken Write(object input) 
		{ 
			return Enum.GetName(enumType, input);
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

		public ClassWriter(Type t)
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
				JToken token = JsonExporter.Export(kvp.Value.GetValue(input));
				if(token != null)
					jObj.Add(kvp.Key, token);
			}
			return jObj;
		}
	}

/*
	public class ClassWriter<T> : Writer<T>
	{
		private Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();

		public ClassWriter()
		{
			foreach(FieldInfo field in typeof(T).GetFields())
			{
				JsonFieldAttribute jsonAttrib = field.GetCustomAttribute<JsonFieldAttribute>();
				if(jsonAttrib != null)
				{
					fields.Add(field.Name, field);
				}
			}
		}

		public override JToken Write(T input) 
		{ 
			JObject jObj = new JObject();
			foreach(var kvp in fields)
			{
				jObj.Add(kvp.Key, JsonExporter.Export(kvp.Value.GetValue(input)));
			}
			return jObj;
		}
	}
	*/

	public static Dictionary<Type, Writer> Writers = null;

	public static JToken Export(object input)
	{
		if(input == null)
			return new JObject();
		return GetOrCreateWriter(input.GetType()).Write(input);
	}

	public static bool CheckInit()
	{
		if(Writers == null)
		{
			Writers = new Dictionary<Type, Writer>();

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

			Writers.Add(typeof(int[]), new ArrayWriter() { ElementWriter = Writers[typeof(int)] });
			Writers.Add(typeof(float[]), new ArrayWriter() { ElementWriter = Writers[typeof(float)] });
			Writers.Add(typeof(double[]), new ArrayWriter() { ElementWriter = Writers[typeof(double)] });
			Writers.Add(typeof(short[]), new ArrayWriter() { ElementWriter = Writers[typeof(short)] });
			Writers.Add(typeof(long[]), new ArrayWriter() { ElementWriter = Writers[typeof(long)] });
			Writers.Add(typeof(byte[]), new ArrayWriter() { ElementWriter = Writers[typeof(byte)] });
			Writers.Add(typeof(string[]), new ArrayWriter() { ElementWriter = Writers[typeof(string)] });
			Writers.Add(typeof(Vector3[]), new ArrayWriter() { ElementWriter = Writers[typeof(Vector3)] });
			Writers.Add(typeof(bool[]), new ArrayWriter() { ElementWriter = Writers[typeof(bool)] });
			Writers.Add(typeof(VecWithOverride[]), new ArrayWriter() { ElementWriter = Writers[typeof(VecWithOverride)]});
		}

		return true;
	}

	public static Writer GetOrCreateWriter(Type type)
	{
		CheckInit();

		if(Writers.TryGetValue(type, out Writer writer))
			return writer;

		if(type.IsArray)
		{
			Writer elementWriter = GetOrCreateWriter(type.GetElementType());
			writer = new ArrayWriter() { ElementWriter = elementWriter };
		}
		else if(type.IsEnum)
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

}
