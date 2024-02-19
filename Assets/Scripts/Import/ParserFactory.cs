using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class ParserFactory
{
	public delegate void ParseFunc(JToken jNode, object target, FieldInfo field, JsonFieldAttribute attrib);
	private interface Parser
	{
		void Parse(JToken jNode, object target, FieldInfo field, JsonFieldAttribute attrib);
	}

	private static Dictionary<Type, ParseFunc> Parsers = null;

	private static void CheckInit()
	{
		if(Parsers == null)
		{
			Parsers = new Dictionary<Type, ParseFunc>();
			Parsers.Add(typeof(short), (jNode, target, field, attrib) => { field.SetValue(target, jNode.ToObject<short>()); });
			Parsers.Add(typeof(int), (jNode, target, field, attrib) => { field.SetValue(target, jNode.ToObject<int>()); });
			Parsers.Add(typeof(float), (jNode, target, field, attrib) => { field.SetValue(target, jNode.ToObject<float>()); });
			Parsers.Add(typeof(double), (jNode, target, field, attrib) => { field.SetValue(target, jNode.ToObject<double>()); });
			Parsers.Add(typeof(string), (jNode, target, field, attrib) => { field.SetValue(target, jNode.ToString()); });
		}
	}

	private static ParseFunc GetOrCreateParseFunc(Type type)
	{
		CheckInit();
		if(Parsers.TryGetValue(type, out ParseFunc result))
			return result;

		if(type.IsEnum)
		{
			string[] names = Enum.GetNames(type);
			Array values = type.GetEnumValues();
			Parsers.Add(typeof(int), 
			(jNode, target, field, attrib) => 
			{ 
				for(int i = 0; i < names.Length; i++)
					if(jNode.ToString() == names[i])
						field.SetValue(target, values.GetValue(i));
			});
		}
		else if(type.IsArray)
		{
			
		}
		else 
		{
			// Add a blank to prevent infinite recursion
			Parsers.Add(type, (jNode, target, field, attrib) => {});
			Parsers.Add(type,
				(jNode, target, field, attrib) => 
				{ 
					JObject jObj = jNode.ToObject<JObject>();
					object targetObject = field.GetValue(target);
					if(targetObject == null)
						targetObject = type.GetConstructor(new Type[0]).Invoke(new object[0]);

					// Now locate all the sub-parsers
					foreach(FieldInfo memberField in type.GetFields())
					{
						JsonFieldAttribute memberAttrib = memberField.GetCustomAttribute<JsonFieldAttribute>();
						if(memberAttrib != null)
						{
							ParseFunc memberParseFunc = GetOrCreateParseFunc(memberField.FieldType);
							if(memberParseFunc != null)
							{
								memberParseFunc.Invoke(jObj[memberField.Name], targetObject, memberField, memberAttrib);
							}
						}
					}

					field.SetValue(target, targetObject);
				});
		}

		return null;
	}


	private static object CreateNewObject(Type type)
	{
		if(typeof(ScriptableObject).IsAssignableFrom(type))
		{
			ScriptableObject element = ScriptableObject.CreateInstance(type);
			element.name = "GeneratedElement";
			//AssetDatabase.CreateAsset(element, )
			return element;
		}

		return null;
	}

	public static void Parse(JObject jObject, Type rootType, object target)
	{
		// Now locate all the sub-parsers
		foreach(FieldInfo memberField in rootType.GetFields())
		{
			JsonFieldAttribute memberAttrib = memberField.GetCustomAttribute<JsonFieldAttribute>();
			if(memberAttrib != null)
			{
				ParseFunc parser = GetOrCreateParseFunc(memberField.FieldType);
				if(parser != null)
				{
					parser.Invoke(jObject, target, memberField, memberAttrib);
				}
			}
		}

	}

	public static void Parse(JObject jObject, FieldInfo field, JsonFieldAttribute attrib)
	{
		//GetOrCreateParseFunc(field.)
	}
    
}
