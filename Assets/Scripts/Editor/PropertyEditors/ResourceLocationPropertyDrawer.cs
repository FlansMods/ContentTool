using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ResourceLocation))]
public class ResourceLocationPropertyDrawer : PropertyDrawer
{
	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		VisualElement container = new VisualElement();

		SerializedProperty namespaceProp = property.FindPropertyRelative("Namespace");
		if (namespaceProp != null)
		{
			List<string> namespaces = new List<string>(new string[] { "minecraft", "forge", "flansmod" });
			foreach (ContentPack pack in ContentManager.inst.Packs)
			{
				if (!namespaces.Contains(pack.ModName))
					namespaces.Add(pack.ModName);
			}
			int index = namespaces.IndexOf(namespaceProp.stringValue);
			DropdownField namespaceDropdown = new DropdownField(
				namespaces,
				index
			);
			namespaceDropdown.BindProperty(namespaceProp);
			container.Add(namespaceDropdown);
		}
		container.Add(new PropertyField(property.FindPropertyRelative("ID")));


		return container;
	}
}
