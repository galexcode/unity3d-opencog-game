/// Unity3D OpenCog World Embodiment Program
/// Copyright (C) 2013  Novamente
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenCog.Attributes;
using OpenCog.Automation;
using OpenCog.Serialization;
using ProtoBuf;
using UnityEditor;
using UnityEngine;
using Enum = System.Enum;
using Type = System.Type;
using TypeCode = System.TypeCode;

namespace OpenCog
{

namespace EditorExtensions
{

/// <summary>
/// The OpenCog Editor.  Expands on inspector interface functionality
/// for scripts.  Exposes properties, fixes missing connections, and
/// allows for custom data representations (such as tooltips).
/// Subclasses will be autogenerated for each Mono Behavior script type
/// by the OpenCog Automated Editor Builder.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[CanEditMultipleObjects]
#endregion
public class OCEditor< OCType >
#region Superclass, Generics, and Interfaces
: Editor
, OCBoolPropertyToggleInterface
, OCEnumPropertyToggleInterface
, OCFloatSliderInterface
, OCIntSliderInterface
, OCTooltipInterface
, OCExposePropertiesInterface
, OCDrawMethodInterface
, OCFixMissingScriptsInterface
where OCType : MonoBehaviour
#endregion
{

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Data

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// The mono behavior instance we're editing.
	/// </summary>
	private OCType _instance;

	private bool _showProperties = false;

	private List<OCPropertyField> _allPropertyFields = new List<OCPropertyField>();

	/// <summary>
	/// Have we tried to find a suitable script for a missing connection?
	/// </summary>
	private static bool _haveTried;

	/// <summary>
	/// The next object to try to find a suitable script for.
	/// </summary>
	private static GameObject _tryThisObject;

	/// <summary>
	/// Are we setup to repaint on changes to the project window?
	/// </summary>
	private static bool _willRepaint = false;

	private Type _type = null;//typeof(OCType);

	private Dictionary<string, bool> _foldedState = new Dictionary<string, bool>();

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Accessors and Mutators

	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Gets or sets a value indicating whether these
	/// <see cref="OpenCog.OCEditor`1"/>s have tried.
	/// </summary>
	/// <value>
	/// <c>true</c> if have tried; otherwise, <c>false</c>.
	/// </value>
	public static bool HaveTried
	{
		get { return _haveTried;}
		set { _haveTried = value;}
	}

	/// <summary>
	/// Gets or sets the next object to try.
	/// </summary>
	/// <value>
	/// The try this object.
	/// </value>
	public static GameObject TryThisObject
	{
		get { return _tryThisObject;}
		set { _tryThisObject = value;}
	}

	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Public Member Functions

	/////////////////////////////////////////////////////////////////////////////

	public void OnEnable()
	{
		_instance = target as OCType;

		OCAutomatedScriptScanner.Init();
	}
 
	public override void OnInspectorGUI()
	{
		// Update the serializedObject - always do this in the beginning of
		// OnInspectorGUI.
		serializedObject.Update();

		EditorGUIUtility.LookLikeInspector();

		DrawInspector(target);

		// Apply changes to the serializedProperty - always do this in the end of
		// OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfDirtyOrScript();

		_allPropertyFields.Clear();

		//@TODO: vvv Is this needed given the repaint event in OnEnable?
		if(GUI.changed)
			EditorUtility.SetDirty(target);
	}

	public void DrawInspector<OCNewType>(OCNewType target)
		where OCNewType : new()
	{
		if(target == null)
		{
			GUILayout.Label("Object is Null");
			return;
		}

		Type exposePropertiesType =
			typeof(OCExposePropertyFieldsAttribute);

		OCExposePropertyFieldsAttribute.OCExposure exposure = OCExposePropertyFieldsAttribute.OCExposure.None;

		if(exposePropertiesType != null)
		{
			object[] attributes =
				target.GetType().GetCustomAttributes(exposePropertiesType, true);

			if(attributes != null && attributes.Length > 0)
				exposure = (attributes[0] as OCExposePropertyFieldsAttribute).Exposure;
		}

		SerializedProperty unityPropertyFieldIterator =
			serializedObject.GetIterator();

		List<OCPropertyField> allPropertyFields =
		OCPropertyField.GetAllPropertiesAndFields
		(
			target
		, unityPropertyFieldIterator
		, exposure
		);

		DrawSerializedProperties(allPropertyFields);

		FindMissingScripts(ref allPropertyFields);

//		OCPropertyField.SetAllPropertiesAndFields
//		(
//			target
//		, allPropertyFields
//		);

	}



	public void DrawSerializedProperties(List< OCPropertyField > allPropertiesAndFields)
	{
		GUIContent label = new GUIContent();
		GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

		//EditorGUILayout.BeginVertical(emptyOptions);

		//Loops through all visible fields
		foreach(OCPropertyField propertyField in allPropertiesAndFields)
		{
			//Debug.Log(propertyField.PublicName + ", " + propertyField.UnityType);

			//if(propertyField.GetValue() == null)
			//	continue;

			//EditorGUILayout.BeginHorizontal(emptyOptions);

			//Finds the bool Condition, enum Condition and tooltip if they exist (They are null otherwise).
			OCBoolPropertyToggleAttribute boolCondition = propertyField.GetAttribute<OCBoolPropertyToggleAttribute>();
			OCEnumPropertyToggleAttribute enumCondition = propertyField.GetAttribute<OCEnumPropertyToggleAttribute>();
			OCDrawMethodAttribute drawMethod = propertyField.GetAttribute<OCDrawMethodAttribute>();
			OCTooltipAttribute tooltip = propertyField.GetAttribute<OCTooltipAttribute>();
			OCFloatSliderAttribute floatSlider = propertyField.GetAttribute<OCFloatSliderAttribute>();
			OCIntSliderAttribute intSlider = propertyField.GetAttribute<OCIntSliderAttribute>();

			//Evaluates the enum and bool conditions
			bool allowedVisibleForBoolCondition = propertyField.IsVisibleForBoolCondition(boolCondition);
			bool allowedVisibleForEnumCondition = propertyField.IsVisibleForEnumCondition(enumCondition);

			//Tests is the field is visible
			if(allowedVisibleForBoolCondition && allowedVisibleForEnumCondition && drawMethod == null)
			{

				label.text = propertyField.PublicName;

				//Sets the tooltip if avaiable
				if(tooltip != null)
				{
					label.tooltip = tooltip.Description;
				}

				DrawFieldInInspector(propertyField, label, emptyOptions, floatSlider, intSlider);

			}
			else
			if(drawMethod != null)
			{
				// If the user wants to draw the field himself.
				MethodInfo drawMethodInfo = this.GetType().GetMethod(drawMethod.DrawMethod);
				if(drawMethodInfo == null)
				{
					Debug.LogError("The '[CustomDrawMethod(" + drawMethod.DrawMethod + "" + drawMethod.ParametersToString() + ")]' failed. Could not find the method '" + drawMethod.DrawMethod + "' in the " + this.ToString() + ". The attribute is attached to the field '" + propertyField.PublicName + "' in '" + propertyField.UnityPropertyField.serializedObject.targetObject + "'.");
					continue;
				}
				ParameterInfo[] parametersInfo = drawMethodInfo.GetParameters();
				if(parametersInfo.Length != (drawMethod.Parameters as object[]).Length)
				{
					Debug.LogError("The '[CustomDrawMethod(" + drawMethod.DrawMethod + "" + drawMethod.ParametersToString() + ")]' failed. The number of parameters in the attribute, did not match the number of parameters in the actual method. The attribute is attached to the field '" + propertyField.PublicName + "' in '" + propertyField.UnityPropertyField.serializedObject.targetObject + "'.");
					continue;
				}

				bool _error = false;
				for(int i = 0; i < parametersInfo.Length; i++)
				{
					//Makes sure the parameter of the actual method is equal to the given parameters
					if(!Type.Equals(parametersInfo[i].ParameterType, drawMethod.Parameters[i].GetType()))
					{
						_error = true;
						Debug.LogError("The '[CustomDrawMethod(" + drawMethod.DrawMethod + "" + drawMethod.ParametersToString() + ")]' failed. The parameter type ('" + drawMethod.Parameters[i].GetType() + "') in the attribute, did not match the the parameter type ('" + parametersInfo[i].ParameterType + "') of the actual method, parameter index: '" + i + "'. The attribute is attached to the field '" + propertyField.PublicName + "' in '" + propertyField.UnityPropertyField.serializedObject.targetObject + "'.");
						continue;
					}
				}
				if(_error)
				{
					continue;
				}

				// VVVVV Calls the users own method  VVVVV
				drawMethodInfo.Invoke(this, drawMethod.Parameters);
				// ^^^^^ Calls the users own method ^^^^^
			}
			else
			{
				//Debug.Log("In OCEditor.DrawSerializedProperties, nothing to draw! " + allowedVisibleForBoolCondition + ", " + allowedVisibleForEnumCondition + ", " + drawMethod);
			}

			//EditorGUILayout.EndHorizontal();

		}

		//EditorGUILayout.EndVertical();
	}

	public void DrawFieldInInspector(OCPropertyField propertyField, GUIContent label, GUILayoutOption[] emptyOptions, OCFloatSliderAttribute floatSlider, OCIntSliderAttribute intSlider)
	{
		if(floatSlider != null)
		{
//			var currentTarget = _instance;
//			MemberInfo[] memberInfo = currentTarget.GetType().GetMember(propertyField.PrivateName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			//Tests if the field is not a float, if so it will display an error
//			if
//			(		memberInfo == null
//			|| ((memberInfo[0] as FieldInfo) == null && (memberInfo[0] as PropertyInfo) == null)
//			|| ((memberInfo[0] as FieldInfo).FieldType != typeof(float) && (memberInfo[0] as PropertyInfo).PropertyType != typeof(float))
//			)
//			{
//				Debug.LogError("The '[FloatSliderInInspector(" + floatSlider.MinValue + " ," + floatSlider.MaxValue + ")]' failed. FloatSliderInInspector does not work with the type '" + memberInfo[0].MemberType + "', it only works with float. The attribute is attached to the field '" + propertyField.Name + "' in '" + _instance + "'.");
//				return;
//			}
			propertyField.SetValue(EditorGUILayout.Slider(label, (float)propertyField.GetValue(), floatSlider.MinValue, floatSlider.MaxValue));

		}
		else
		if(intSlider != null)
		{
//			var currentTarget = _instance;
//			MemberInfo[] memberInfo = currentTarget.GetType().GetMember(propertyField.PrivateName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			//Tests if the field is not a int, if so it will display an error
//			if
//			(		memberInfo == null
//			|| ((memberInfo[0] as FieldInfo) == null && (memberInfo[0] as PropertyInfo) == null)
//			|| ((memberInfo[0] as FieldInfo).FieldType != typeof(int) && (memberInfo[0] as PropertyInfo).PropertyType != typeof(int))
//			)
//			{
//				Debug.LogError("The '[IntSliderInInspector(" + intSlider.MinValue + " ," + intSlider.MaxValue + ")]' failed. IntSliderInInspector does not work with the type '" + memberInfo[0].MemberType + "', it only works with int. The attribute is attached to the field '" + propertyField.Name + "' in '" + _instance + "'.");
//				return;
//			}
			propertyField.SetValue(EditorGUILayout.IntSlider(label, (int)propertyField.GetValue(), intSlider.MinValue, intSlider.MaxValue));
		}
		else
		if(propertyField.UnityPropertyField != null && propertyField.UnityPropertyField.editable)
		{
			// VVVV DRAWS THE STANDARD FIELD  VVVV
			EditorGUILayout.PropertyField(propertyField.UnityPropertyField, label, true);
			// ^^^^^  DRAWS THE STANDARD FIELD  ^^^^^
		}
		else
		{
			if(propertyField.UnityPropertyField != null)
			{
				switch(propertyField.UnityType)
				{
				case SerializedPropertyType.Integer:
					propertyField.SetValue(EditorGUILayout.IntField(label, (int)propertyField.GetValue(), emptyOptions));
					break;
		
				case SerializedPropertyType.Float:
					propertyField.SetValue(EditorGUILayout.FloatField(label, (float)propertyField.GetValue(), emptyOptions));
					break;

				case SerializedPropertyType.Boolean:
					propertyField.SetValue(EditorGUILayout.Toggle(label, (bool)propertyField.GetValue(), emptyOptions));
					break;

				case SerializedPropertyType.String:
					propertyField.SetValue(EditorGUILayout.TextField(label, (string)propertyField.GetValue(), emptyOptions));
					break;
		
				case SerializedPropertyType.Vector2:
					propertyField.SetValue(EditorGUILayout.Vector2Field(propertyField.PublicName, (Vector2)propertyField.GetValue(), emptyOptions));
					break;
		
				case SerializedPropertyType.Vector3:
					propertyField.SetValue(EditorGUILayout.Vector3Field(propertyField.PublicName, (Vector3)propertyField.GetValue(), emptyOptions));
					break;
		
				case SerializedPropertyType.Enum:
					propertyField.SetValue(EditorGUILayout.EnumPopup(label, (Enum)propertyField.GetValue(), emptyOptions));
					break;
		
				case SerializedPropertyType.Generic:
				case SerializedPropertyType.ObjectReference:

					if(propertyField.PublicName == "Animation")
						Debug.Log("Drawing Animation: " + propertyField.CSType);

					if(propertyField.CSType.IsSerializable)
					{
						List<OCPropertyField> nestedPropertiesAndFields = new List<OCPropertyField>();

						nestedPropertiesAndFields = OCPropertyField.GetAllPropertiesAndFields
						( propertyField.Instance
						, propertyField.UnityPropertyField
						, OCExposePropertyFieldsAttribute.OCExposure.PropertiesAndFields
						);

						Debug.Log("In OCEditor.DrawFieldInInspector, propertyField type is Serializable.");

						Debug.Log("In OCEditor.DrawFieldInInspector, nested Properties and Fields: " + nestedPropertiesAndFields.Select(p => p.PublicName).Aggregate((a, b) => a + ", " + b));

						DrawSerializedProperties(nestedPropertiesAndFields);

					}
					break;
		
				default:
		
					break;

				}
			}
			else if(propertyField.MemberInfo != null)
			{
				//Debug.Log(propertyField.PublicName + ", " + propertyField.Instance);
				switch(Type.GetTypeCode(propertyField.CSType))
				{
					case TypeCode.Boolean:
						propertyField.SetValue(EditorGUILayout.Toggle(label, (bool)propertyField.GetValue(), emptyOptions));
						break;
			
					case TypeCode.Decimal:
					case TypeCode.Double:
					case TypeCode.Single:
						propertyField.SetValue(EditorGUILayout.FloatField(label, (float)propertyField.GetValue(), emptyOptions));
						break;
			
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
						propertyField.SetValue(EditorGUILayout.IntField(label, (int)propertyField.GetValue(), emptyOptions));
						break;
			
					case TypeCode.String:
						propertyField.SetValue(EditorGUILayout.TextField(label, (string)propertyField.GetValue(), emptyOptions));
						break;

					case TypeCode.Object:
						//Debug.Log("Drawing: " + propertyField.CSType + ", " + propertyField.CSType.IsSerializable);

						if(propertyField.CSType.IsSerializable && (propertyField.Instance != null || (propertyField.UnityPropertyField != null && propertyField.UnityPropertyField.objectReferenceValue != null)))
						{
							//	DrawDefaultInspector();
							List<OCPropertyField> nestedPropertiesAndFields = new List<OCPropertyField>();

							nestedPropertiesAndFields = OCPropertyField.GetAllPropertiesAndFields
							( propertyField.GetValue()
							, propertyField.UnityPropertyField
							, OCExposePropertyFieldsAttribute.OCExposure.PropertiesAndFields
							);

							//EditorGUILayout.Separator();

							//GUILayout.Space(-14);



							if(_foldedState.ContainsKey(propertyField.PublicName))
							{
								EditorGUI.indentLevel--;
								_foldedState[propertyField.PublicName] = EditorGUILayout.Foldout(_foldedState[propertyField.PublicName], label);
								EditorGUI.indentLevel++;
							}
							else
							{
								_foldedState.Add(propertyField.PublicName, true);
							}

							if(_foldedState[propertyField.PublicName])
							{

								//EditorGUILayout.BeginVertical();
								//GUILayout.Space(15);
								//EditorGUILayout.BeginHorizontal();
								//GUILayout.Space(-25);
//								Debug.Log("In OCEditor.DrawFieldInInspector, propertyField type is Serializable.");
//
//								Debug.Log("In OCEditor.DrawFieldInInspector, nested Properties and Fields: " + nestedPropertiesAndFields.Select(p => p.Instance != null ? p.Instance.ToString() : "Null").Aggregate((a, b) => a + ", " + b));

								EditorGUI.indentLevel++;

								DrawSerializedProperties(nestedPropertiesAndFields);

								EditorGUI.indentLevel--;

								//EditorGUILayout.EndHorizontal();
								//EditorGUILayout.EndVertical();

							}
						}
						else if(propertyField.CSType.IsSubclassOf(typeof(UnityEngine.Object)))
						{
							propertyField.SetValue(EditorGUILayout.ObjectField(label, (UnityEngine.Object)propertyField.GetValue(), propertyField.CSType, true, emptyOptions));
						}
						else if(propertyField.CSType.IsEnum)
						{
							propertyField.SetValue(EditorGUILayout.EnumPopup(label, (Enum)propertyField.GetValue(), emptyOptions));
						}
						break;

					default:
						break;
				}
				if(propertyField.CSType.IsEnum)
					propertyField.SetValue(EditorGUILayout.EnumPopup(label, (Enum)propertyField.GetValue(), emptyOptions));
			}
		}
	}
   
	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

  #region Private Member Functions

	/////////////////////////////////////////////////////////////////////////////

	private void FindMissingScripts(ref List< OCPropertyField > allPropertyFields)
	{
		OCPropertyField scriptPropertyField =
			_allPropertyFields.Find(p => p.PublicName == "Script");

		if(scriptPropertyField != null
		&& scriptPropertyField != default(OCPropertyField))
		{
			// Tests if there is a missing script
			MonoScript sourceScript = (MonoScript)scriptPropertyField.GetValue();
			if(sourceScript == null)
			{

				EditorPrefs.SetBool("Fix", GUILayout.Toggle(EditorPrefs.GetBool("Fix", true), "Fix broken scripts"));
				if(!EditorPrefs.GetBool("Fix", true))
				{
					GUILayout.Label("*** SCRIPT MISSING ***");
					return;
				}
		
		//		List<OCPropertyField> allPropertyFieldsCopy = new List<OCPropertyField>(allPropertyFields);
		
				foreach(OCPropertyField propertyField in allPropertyFields)
				{
					//Debug.Log("In OCEditor.FindMissingScripts(), property name: " + property.name);
					if(propertyField.PublicName == "Script" && propertyField.MemberInfo == null)
					{
						//Debug.Log("In OCEditor.FindMissingScripts(), found script");
						Component targetComponent = target as Component;
						if(targetComponent != null && TryThisObject == targetComponent.gameObject)
						{
							//Debug.Log("In OCEditor.FindMissingScripts(), we have tried this script already");
							HaveTried = true;
						}
		
						List< OCScript > candidates = OCAutomatedScriptScanner.Scripts.ToList();
		
						foreach(OCPropertyField subPropertyField in allPropertyFields)
						{
							//Debug.Log("SubPropertyField Name: " + subPropertyField.PublicName);
		
							if(candidates.Count == 0)
							{
								//Debug.Log("candidates = 0");
								break;
							}
		
							if(subPropertyField.PublicName != "Script"
							&& subPropertyField.PrivateName != propertyField.PrivateName)
							{
								//Debug.Log("Before selection: " + candidates.Count);
								candidates = candidates.Where(c => c.Properties.ContainsKey(subPropertyField.PublicName)).ToList();
								//Debug.Log("After  selection: " + candidates.Count); 
							}
						}
		
						if(candidates.Count == 1)
						{
							propertyField.SetValue(candidates[0].Script);
		
							serializedObject.ApplyModifiedProperties();
							serializedObject.UpdateIfDirtyOrScript();
		
						}
						else
						if(candidates.Count > 0)
						{
							foreach(OCScript candidate in candidates)
							{
								if(candidate != null && candidate.Script != null && GUILayout.Button("Use " + candidate.Script.name))
								{
									//Configure the script
									propertyField.SetValue(candidate.Script);
									_type = candidate.Script.GetClass();

									//if(_instance != null) UnityEditor.EditorUtility.SetDirty(_instance);
		
									serializedObject.ApplyModifiedProperties();
									serializedObject.UpdateIfDirtyOrScript();
		
									//_editor = (OCDefaultEditor)Editor.CreateEditor(target);
									//_editor.OnEnable();
									//_editor._allPropertyFields = candidate.Properties.Values.ToList();
									//_editor. .OnEnable();
		
									//if(candidate.Script != null)
									{
										//Debug.Log("Creating a new editor.");
										//UnityEditor.EditorWindow.
		
										//EditorUtility.SetDirty(target);
									}
		
//							System.Type type = default(Type);
//
//							Assembly []referencedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
//							for(int i = 0; i < referencedAssemblies.Length; ++i)
//							{
//							  type = referencedAssemblies[i].GetType( "UnityEditor.InspectorWindow" );
//
//							  if( type != null )
//							  {   // I want all the declared methods from the specific class.
//							      //System.Reflection.MethodInfo []methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
//										Debug.Log("Found Type: " + type);
//										break;
//							  }
//							}



							//Editor.DestroyImmediate(this);

							//EditorWindow.focusedWindow.Repaint();

							//this.DrawDefaultInspector();

							//UnityEditor.InspectorWindow.DrawEditors

							//UnityEditor.EditorWindow.GetWindow(Type.GetType("UnityEditor.InspectorWindow")).Repaint();

							//EditorApplication.ExecuteMenuItem("Window/Hierarchy");

//							EditorWindow inspector = (Resources.FindObjectsOfTypeAll(type) as EditorWindow[]).FirstOrDefault();//(Editor.FindObjectsOfTypeIncludingAssets(typeof(EditorWindow)) as EditorWindow[]).Where(x => x.GetType().ToString() == "UnityEditor.InspectorWindow").FirstOrDefault();
//
//							if(inspector != default(EditorWindow))
//								Debug.Log("Inspector: " + inspector.ToString());
//
//							System.Reflection.MethodInfo []methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
//
//							foreach(System.Reflection.MethodInfo method in methods)
//							{
//								Debug.Log("---" + method.Name);
//								if(method.Name == "GetInspectedObject")
//								{
////									ParameterInfo[] infos = method.GetParameters();
////									foreach(ParameterInfo info in infos)
////									{
////										Debug.Log("-----" + info.ParameterType + ", " + info.Name);
////									}
////									Editor[] editors = {this};
////									object[] parameters = {editors, 0};
////									method.Invoke(inspector, parameters);
////									GameObject o = (GameObject)method.Invoke(inspector, null);
////									Debug.Log("What am I? : " + o.GetType());
////									Editor editor = (Editor)o;
////									editor.
//								}
//							}

//							inspector.Close();
//							EditorApplication.ExecuteMenuItem("Window/Inspector");
//							inspector.Show();
		
									return;
		    
								}
							}
						}
						else
						{
							GUILayout.Label("> No suitable scripts were found");
						}
						break;
					}
				}

			}
			else
			{
				OCScript targetScript =
					OCAutomatedScriptScanner.Scripts.Find
					(
						s => s.Script.name == sourceScript.name
					)
				;

				if(_allPropertyFields == null)
					Debug.Log("In OCEditor.OnInspectorGUI, no property fields!");
				targetScript.Properties = _allPropertyFields.ToDictionary(p => p.PrivateName);
			}
		}
	}

//	private bool IsScriptMissing(OCPropertyField scriptPropertyField)
//	{
//		//@TODO: fix this test
//		return target.GetType() != typeof(OCType) || scriptPropertyField.GetValue() == null;
//	}

	//@TODO: Finish this function...

	private void SerializeAndHidePrivateDataMembers(System.Object obj)
	{
		if(obj == null)
		{
			return;
		}
 
		List< FieldInfo > fields = new List<FieldInfo>();
 
		Type objType = obj.GetType();

		FieldInfo[] infos = objType.GetFields
    (
      BindingFlags.NonPublic
    | BindingFlags.Instance
    );

		foreach(FieldInfo info in infos)
		{

			object[] attributes = info.GetCustomAttributes(true);

		}


	}







	/////////////////////////////////////////////////////////////////////////////

  #endregion

	/////////////////////////////////////////////////////////////////////////////

}// class OCEditor

}// namespace EditorExtensions

}// namespace OpenCog