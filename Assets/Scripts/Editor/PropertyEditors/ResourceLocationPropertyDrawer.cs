using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ResourceLocation))]
public class ResourceLocationPropertyDrawer : PropertyDrawer
{
	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		// Load from Resources
		var asset = Resources.Load<VisualTreeAsset>("resource_location_drawer");
		var drawer = asset.Instantiate(property.propertyPath);

		SerializedProperty namespaceProp = property.FindPropertyRelative("Namespace");
		SerializedProperty idProp = property.FindPropertyRelative("ID");

		string assetPathHint = "";
		if (property.serializedObject.targetObjects.Length <= 1)
		{
			JsonFieldAttribute jsonFieldAttrib = property.GetUnderlyingField().GetAttribute<JsonFieldAttribute>();
			if (jsonFieldAttrib != null)
				assetPathHint = jsonFieldAttrib.AssetPathHint;
		}

		drawer.Q<Label>().text = property.displayName;

		drawer.Q<TextField>("NamespaceEntry")?.BindProperty(namespaceProp);
		drawer.Q<TextField>("IDEntry")?.BindProperty(idProp);

		DropdownField namespaceDropdown = drawer.Q<DropdownField>("NamespaceDropdown");
		if(namespaceDropdown != null)
		{
			namespaceDropdown.choices = ResourceLocation.Namespaces;
			namespaceDropdown.BindProperty(namespaceProp);
			namespaceDropdown.RegisterValueChangedCallback((changeEvent) =>
			{
				if(changeEvent.newValue == ResourceLocation.NEW_NAMESPACE)
				{
					// TODO:
				}

				RefreshIDChoices(drawer, changeEvent.newValue, assetPathHint);
			});
			namespaceDropdown.formatSelectedValueCallback = (toFormat) => { return ""; };
		}

		DropdownField idDropdown = drawer.Q<DropdownField>("IDDropdown");
		if (idDropdown != null)
		{
			ContentPack pack = ContentManager.inst.FindContentPack(namespaceProp.stringValue);
			if(pack != null)
			{
				if(assetPathHint.Length > 0)
				{
					idDropdown.choices = new List<string>(pack.IDsWithPrefix(assetPathHint));
				}
				else idDropdown.choices = new List<string>(pack.AllIDs);
			}
			else
			{
				idDropdown.choices = new List<string>(new string[] { "Pack not found" });
			}
			idDropdown.BindProperty(idProp);
			idDropdown.formatSelectedValueCallback = (toFormat) => { return ""; };
		}

		return drawer;
	}

	private void RefreshIDChoices(VisualElement drawer, string newValue, string assetPathHint)
	{
		DropdownField idDropdown = drawer.Q<DropdownField>("IDDropdown");
		if (idDropdown != null)
		{
			ContentPack pack = ContentManager.inst.FindContentPack(newValue);
			if (pack != null)
			{
				if (assetPathHint.Length > 0)
				{
					idDropdown.choices = new List<string>(pack.IDsWithPrefix(assetPathHint));
				}
				else idDropdown.choices = new List<string>(pack.AllIDs);
			}
			else
			{
				idDropdown.choices = new List<string>(new string[] { "Pack not found" });
			}
		}
	}
}
