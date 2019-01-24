using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;
using Vertx.Constants;

namespace Vertx
{
	[EditorTool("Kittens!")]
	public class KittensEditorTool : EditorTool
	{
		private GUIContent _iconContent;
		public override GUIContent toolbarIcon => _iconContent;

		void OnEnable() => _iconContent = new GUIContent(LoadTextureFromString(KittensImages.Icon), "Kittens!");

		private static Texture2D LoadTextureFromString(string s)
		{
			byte[] rawTextureData = System.Convert.FromBase64String(s);
			Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			tex.LoadImage(rawTextureData);
			return tex;
		}

		public override void OnActivate()
		{
			DiscoverAllRootContexts();
		}

		public override void OnToolGUI(EditorWindow window)
		{
			
		}

		List<VisualElement> DiscoverAllRootContexts()
		{
			Type uiElementsUtilityType = Type.GetType("UnityEngine.UIElements.UIElementsUtility,UnityEngine");
			if (!DebugRelevantError(uiElementsUtilityType, "UnityEngine.UIElements.UIElementsUtility", "type"))
				return null;

			Type panelType = Type.GetType("UnityEngine.UIElements.Panel,UnityEngine");
			if (!DebugRelevantError(panelType, "UnityEngine.UIElements.Panel", "type"))
				return null;
			
			Type dictionaryType = typeof(KeyValuePair<,>);
			Type genericDictionaryType = dictionaryType.MakeGenericType(typeof(int), panelType);
			PropertyInfo kvpValueProperty = genericDictionaryType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
			if (!DebugRelevantError(kvpValueProperty, "KeyValuePair<int,Panel>.Value", "Property"))
				return null;

			MethodInfo GetPanelsIteratorMethod = uiElementsUtilityType.GetMethod("GetPanelsIterator", BindingFlags.Static | BindingFlags.NonPublic);
			if (!DebugRelevantError(GetPanelsIteratorMethod, "UIElementsUtility.GetPanelsIterator", "Method"))
				return null;

			IEnumerator enumerator = (IEnumerator) GetPanelsIteratorMethod.Invoke(null, null);

			//Get all panels to inject content into
			while (enumerator.MoveNext())
			{
				object enumeratorCurrent = enumerator.Current;
				object value = kvpValueProperty.GetValue(enumeratorCurrent);
				Debug.Log(value);
			}
			
			return null;

			bool DebugRelevantError(object o, string name, string type)
			{
				if (o == null)
				{
					Debug.LogError($"{type} {name} not found.");
					return false;
				}
				return true;
			}
		}

		private void OnDisable()
		{
			DestroyImmediate(_iconContent.image);
		}
	}
}