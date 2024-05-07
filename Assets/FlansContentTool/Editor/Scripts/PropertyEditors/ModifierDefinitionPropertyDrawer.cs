using Codice.CM.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ModifierDefinition))]
public class ModifierDefinitionPropertyDrawer : PropertyDrawer
{
	private List<string> PossibleModifiers = new List<string>(Constants.STAT_SUGGESTIONS);
	private const string Custom = "Custom ...";

	public ModifierDefinitionPropertyDrawer()
	{
		PossibleModifiers.Sort();
		PossibleModifiers.Add(Custom);
	}

	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		// Load from Resources
		var asset = Resources.Load<VisualTreeAsset>("modifier_definition_drawer");
		var drawer = asset.Instantiate(property.propertyPath);

		SerializedProperty statProp = property.FindPropertyRelative("stat");
		SerializedProperty matchGroupPathsProp = property.FindPropertyRelative("matchGroupPaths");
		SerializedProperty accumulatorsProp = property.FindPropertyRelative("accumulators");
		SerializedProperty setValueProp = property.FindPropertyRelative("setValue");

		TextField statField = drawer.Q<TextField>("StatEntry");
		DropdownField statDropdown = drawer.Q<DropdownField>("StatDropdown");
		if(statField != null && statDropdown != null)
		{
			statField.BindProperty(statProp);

			statDropdown.choices = PossibleModifiers;
			statDropdown.BindProperty(statProp);
			statDropdown.RegisterValueChangedCallback((changeEvent) =>
			{
				if (changeEvent.newValue == Custom)
					statProp.stringValue = "";
			});
		}

		ListView listView = drawer.Q<ListView>("Accumulators");
		TextField setValueField = drawer.Q<TextField>("SetValueEntry");
		if(setValueField != null)
		{
			setValueField.BindProperty(setValueProp);

			if(listView != null)
			{
				setValueField.RegisterValueChangedCallback((changeEvent) =>
				{
					if (changeEvent.newValue.Length > 0)
					{
						listView.AddToClassList("unity-disabled");
					}
					else
					{
						listView.RemoveFromClassList("unity-disabled");
					}
				});
			}
		}

		
		if(listView != null)
		{
			if(setValueProp.stringValue.Length > 0)
			{
				listView.AddToClassList("unity-disabled");
			}

			var elementAsset = Resources.Load<VisualTreeAsset>("stat_accumulator_definition_drawer");

			List<SerializedProperty> props = new List<SerializedProperty>();
			for (int i = 0; i < accumulatorsProp.arraySize; i++)
				props.Add(accumulatorsProp.GetArrayElementAtIndex(i));

			listView.itemsSource = props;


			listView.makeItem = () => { return elementAsset.Instantiate(); };
			listView.bindItem = (element, index) => 
			{ 
				if(element != null && index < accumulatorsProp.arraySize)
					StatAccumulatorDefinitionPropertyDrawer.Bind(element, accumulatorsProp.GetArrayElementAtIndex(index));
			};


			listView.itemsAdded += (indices) =>
			{
				foreach (int index in indices)
				{
					accumulatorsProp.arraySize++;
					accumulatorsProp.GetArrayElementAtIndex(accumulatorsProp.arraySize - 1).boxedValue = new StatAccumulatorDefinition();
				}

				accumulatorsProp.serializedObject.ApplyModifiedProperties();
				listView.RefreshItems();
			};

			// OOOF why is this so stupid
			listView.itemsRemoved += (indices) =>
			{
				accumulatorsProp.arraySize--;
				accumulatorsProp.serializedObject.ApplyModifiedProperties();
				listView.RefreshItems();

			};
		}
	

		return drawer;
	}
}
