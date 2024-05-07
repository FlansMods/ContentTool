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

[CustomPropertyDrawer(typeof(StatAccumulatorDefinition))]
public class StatAccumulatorDefinitionPropertyDrawer : PropertyDrawer
{
	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		// Load from Resources
		var asset = Resources.Load<VisualTreeAsset>("stat_accumulator_definition_drawer");
		var drawer = asset.Instantiate(property.propertyPath);

		Bind(drawer, property);

		return drawer;
	}

	public static void Bind(VisualElement drawer, SerializedProperty property)
	{
		SerializedProperty operationProp = property.FindPropertyRelative("operation");
		SerializedProperty valueProp = property.FindPropertyRelative("value");
		SerializedProperty multiplyPerProp = property.FindPropertyRelative("multiplyPer");

		DropdownField opDropdown = drawer.Q<DropdownField>("OperationDropdown");
		if (opDropdown != null)
		{
			opDropdown.BindProperty(operationProp);
		}

		FloatField valueField = drawer.Q<FloatField>("ValueField");
		if (valueField != null)
		{
			valueField.BindProperty(valueProp);
		}

		BindSourceToggle(drawer.Q<Toggle>("xLevel"), multiplyPerProp, EAccumulationSource.PerLevel);
		BindSourceToggle(drawer.Q<Toggle>("xStacks"), multiplyPerProp, EAccumulationSource.PerStacks);
		BindSourceToggle(drawer.Q<Toggle>("xAttachments"), multiplyPerProp, EAccumulationSource.PerAttachment);
		BindSourceToggle(drawer.Q<Toggle>("xMagFullness"), multiplyPerProp, EAccumulationSource.PerMagFullness);
		BindSourceToggle(drawer.Q<Toggle>("xMagEmptiness"), multiplyPerProp, EAccumulationSource.PerMagEmptiness);

	}

	private static void BindSourceToggle(Toggle toggle, SerializedProperty multiplyPerProp, EAccumulationSource sourceType)
	{
		if (toggle != null)
		{
			toggle.style.flexShrink = 1;
			toggle.labelElement.style.flexShrink = 1;
			toggle.labelElement.style.minWidth = 40;

			bool containsSource = false;
			for (int i = multiplyPerProp.arraySize - 1; i >= 0; i--)
			{
				if (multiplyPerProp.GetArrayElementAtIndex(i).enumValueIndex == (int)sourceType)
				{
					containsSource = true;
				}
			}
			toggle.value = containsSource;

			toggle.RegisterValueChangedCallback(changeEvent =>
			{
				for (int i = multiplyPerProp.arraySize - 1; i >= 0; i--)
				{
					if (multiplyPerProp.GetArrayElementAtIndex(i).enumValueIndex == (int)sourceType)
					{
						// If toggling off, remove the old variable
						if (!changeEvent.newValue)
							multiplyPerProp.DeleteArrayElementAtIndex(i);
						else
							// If toggling on, don't add it if its already on
							return;
					}
				}

				if (changeEvent.newValue)
				{
					multiplyPerProp.InsertArrayElementAtIndex(multiplyPerProp.arraySize);
					multiplyPerProp.GetArrayElementAtIndex(multiplyPerProp.arraySize - 1).enumValueIndex = (int)sourceType;
				}

				multiplyPerProp.serializedObject.ApplyModifiedProperties();
			});
		}
	}
}
