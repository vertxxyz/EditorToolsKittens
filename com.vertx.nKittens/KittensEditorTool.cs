using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;
using Vertx.Constants;
using Random = UnityEngine.Random;

namespace Vertx
{
	[EditorTool("Kittens!")]
	public class KittensEditorTool : EditorTool
	{
		private GUIContent _iconContent;
		public override GUIContent toolbarIcon => _iconContent;

		void OnEnable() => _iconContent = new GUIContent(LoadTextureFromString(KittensImages.Icon), "Kittens!");
		
		private void OnDisable() => DestroyImmediate(_iconContent.image);
				
		public override void OnActivate()
		{
			iterateOnUpdate = IterateOnUpdate();
			updateStartTime = Time.realtimeSinceStartup;
			waitToTime = -1;
			EditorApplication.update += Update;
		}

		public override void OnDeactivate()
		{
			iterateOnUpdate = null;
			EditorApplication.update -= Update;
		}

		#region Iterator Logic

		private IEnumerator iterateOnUpdate;
		private double waitToTime;
		private double updateStartTime;
		private void Update()
		{
			double updateTime = Time.realtimeSinceStartup - updateStartTime;
			
			if (waitToTime > updateTime)
				return;

			if (iterateOnUpdate.MoveNext())
			{
				switch (iterateOnUpdate.Current)
				{
					case WaitForSeconds wait:
						waitToTime = updateTime + wait.Seconds;
						return;
					case null:
						return;
					default:
						throw new NotImplementedException();
				}
			}
		}
		
		private struct WaitForSeconds
		{
			public readonly float Seconds;

			/// <summary>
			/// Creates a yield instruction to wait for a given number of seconds
			/// </summary>
			/// <param name="seconds">Seconds to wait</param>
			public WaitForSeconds(float seconds) => Seconds = seconds;
		}
		#endregion
		
		private const float initialWaitTimeMin = 0;
		private const float initialWaitTimeMax = 5;
		
		private const float holdTimeMin = 5;
		private const float holdTimeMax = 10;
		
		private const float waitTimeMin = 0;
		private const float waitTimeMax = 10;

		IEnumerator IterateOnUpdate()
		{
			Debug.Log("Start");
			float initialWait = Random.Range(initialWaitTimeMin, initialWaitTimeMax);
			yield return new WaitForSeconds(initialWait);
			Debug.Log($"Waited for {initialWait} seconds");
			while (true)
			{
				//KITTEN EMERGES
				List<VisualElement> rootContexts = DiscoverAllRootContexts();
				//TODO insert 
				
				//WAIT FOR KITTEN TO LEAVE
				float hold = Random.Range(holdTimeMin, holdTimeMax);
				yield return new WaitForSeconds(hold);
				//TODO remove
				
				//WAIT FOR NEXT KITTEN TO EMERGE
				float wait = Random.Range(waitTimeMin, waitTimeMax);
				yield return new WaitForSeconds(wait);
			}
			// ReSharper disable once IteratorNeverReturns
		}
		#region Helpers
		private static Texture2D LoadTextureFromString(string s)
		{
			byte[] rawTextureData = Convert.FromBase64String(s);
			Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
			tex.LoadImage(rawTextureData);
			return tex;
		}
		
		static List<VisualElement> DiscoverAllRootContexts()
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

			PropertyInfo visualTreeProperty = panelType.GetProperty("visualTree", BindingFlags.Public | BindingFlags.Instance);
			if (!DebugRelevantError(kvpValueProperty, "Panel / BaseVisualElementPanel.visualTree", "Property"))
				return null;

			IEnumerator enumerator = (IEnumerator) GetPanelsIteratorMethod.Invoke(null, null);

			List<VisualElement> roots = new List<VisualElement>();
			//Get all panels' root VisualElements to inject content into
			while (enumerator.MoveNext())
			{
				object enumeratorCurrent = enumerator.Current;
				object panel = kvpValueProperty.GetValue(enumeratorCurrent);
				VisualElement visualTreeRoot = visualTreeProperty.GetValue(panel) as VisualElement;
				if(visualTreeRoot == null) continue;
				roots.Add(visualTreeRoot);
			}
			return roots;

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
		#endregion
	}
}