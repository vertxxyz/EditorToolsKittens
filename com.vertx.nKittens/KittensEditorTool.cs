//#define VERBOSE_DEBUGGING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.Constants.KittensImages;
using Random = UnityEngine.Random;

namespace Vertx
{
	[EditorTool("Kittens!")]
	public class KittensEditorTool : EditorTool
	{
		private GUIContent _iconContent;
		public override GUIContent toolbarIcon => _iconContent;

		void OnEnable()
		{
			_iconContent = new GUIContent(LoadTextureFromString(Icon), "Kittens!"); // Very sad 🐱 does not display properly. Very funny that this can inject a kitten into a tooltip though.

			kittenVisualElements = new Image[kittens.Length];
			for (int i = 0; i < kittenVisualElements.Length; i++)
			{
				Texture2D kitten = LoadTextureFromString(kittens[i].Image);
				kitten.name = $"Kitten {i}";
				kittenVisualElements[i] = new Image
				{
					image = kitten,
					scaleMode = ScaleMode.StretchToFill,
					style =
					{
						position = Position.Absolute
					}
				};
			}
			
			#if UNITY_2019_2_OR_NEWER
			EditorTools.activeToolChanged += OnToolChange;
			EditorTools.activeToolChanging += OnToolChanging;
			if (EditorTools.IsActiveTool(this))
				Activate();
			#endif
		}

		[Shortcut("Kittens!", KeyCode.K, ShortcutModifiers.Action)]
		private static void EnableKittens() => EditorTools.SetActiveTool<KittensEditorTool>();

		void RemoveAllKittensFromHierarchies()
		{
			foreach (Image kittenVisualElement in kittenVisualElements)
				kittenVisualElement.RemoveFromHierarchy();
		}

		private void OnDisable()
		{
			DestroyImmediate(_iconContent.image);
			RemoveAllKittensFromHierarchies();
			foreach (Image kittenVisualElement in kittenVisualElements)
				DestroyImmediate(kittenVisualElement.image);
			
			#if UNITY_2019_2_OR_NEWER
			EditorTools.activeToolChanged -= OnToolChange;
			EditorTools.activeToolChanging -= OnToolChanging;
			#endif
		}
		
		#if UNITY_2019_2_OR_NEWER
		void OnToolChange()
		{
			if (EditorTools.IsActiveTool(this))
				Activate();
		}
		
		void OnToolChanging()
		{
			if (EditorTools.IsActiveTool(this))
				Deactivate();
		}
		#endif

		#if !UNITY_2019_2_OR_NEWER
		public override void OnActivate() => Activate();
		
		public override void OnDeactivate() => Deactivate();
		#endif

		void Activate()
		{
			#if VERBOSE_DEBUGGING
			Debug.Log("Activate");
			#endif
			
			iterateOnUpdate = IterateOnUpdate();
			updateStartTime = Time.realtimeSinceStartup;
			waitToTime = -1;
			EditorApplication.update += Update;
		}

		void Deactivate()
		{
			#if VERBOSE_DEBUGGING
			Debug.Log("Deactivate");
			#endif
			
			iterateOnUpdate = null;
			EditorApplication.update -= Update;
			RemoveAllKittensFromHierarchies();
		}

		#region Iterator Logic

		private IEnumerator iterateOnUpdate;
		private float waitToTime;
		private float updateStartTime;

		private void Update()
		{
			float updateTime = Time.realtimeSinceStartup - updateStartTime;
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

		private const float holdTimeMin = 2;
		private const float holdTimeMax = 10;

		private const float waitTimeMin = 0;
		private const float waitTimeMax = 40;

		private Image[] kittenVisualElements;

		private const float emergeTime = 0.5f;
		private const float leaveTime = 1f;
		private readonly AnimationCurve curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

		IEnumerator IterateOnUpdate()
		{
			#if VERBOSE_DEBUGGING
			Debug.Log("Start");
			#endif

			float initialWait = Random.Range(initialWaitTimeMin, initialWaitTimeMax);
			yield return new WaitForSeconds(initialWait);

			#if VERBOSE_DEBUGGING
			Debug.Log($"Waited for {initialWait} seconds");
			#endif

			while (true)
			{
				#if VERBOSE_DEBUGGING
				Debug.Log("Emerge");
				#endif

				//KITTEN EMERGES
				List<RootWithName> rootContexts = DiscoverAllRootContexts();
				if (rootContexts == null)
					yield break;

				int randomIndex = Random.Range(0, rootContexts.Count);
				#if VERBOSE_DEBUGGING
				Debug.Log(rootContexts[randomIndex].name);
				#endif
				VisualElement randomRoot = rootContexts[randomIndex].root;
				//Images don't render without doing this in 2019.2
				randomRoot = randomRoot.Children().Last();

				Image kittenVisualElement = kittenVisualElements[Random.Range(0, kittenVisualElements.Length)];
				
				randomRoot.Add(kittenVisualElement);
				float wRatio = Mathf.Min(kittenVisualElement.image.width, randomRoot.layout.width) / kittenVisualElement.image.width;
				float hRatio = Mathf.Min(kittenVisualElement.image.height, randomRoot.layout.height) / kittenVisualElement.image.height;
				float minRatio = Mathf.Min(wRatio, hRatio);
				float w = kittenVisualElement.image.width * minRatio;
				float h = kittenVisualElement.image.height * minRatio;
				kittenVisualElement.style.width = w;
				kittenVisualElement.style.height = h;

				kittenVisualElement.style.bottom = StyleKeyword.None;
				kittenVisualElement.style.top = StyleKeyword.None;
				kittenVisualElement.style.left = StyleKeyword.None;
				kittenVisualElement.style.right = StyleKeyword.None;

				int xRandom = Mathf.RoundToInt(Random.value);
				Action<float> setX;
				if (xRandom == 0)
				{
					setX = v => kittenVisualElement.style.left = v;
					Rect rect = kittenVisualElement.uv;
					rect.width = 1;
					kittenVisualElement.uv = rect;
				}
				else
				{
					setX = v => kittenVisualElement.style.right = v;
					Rect rect = kittenVisualElement.uv;
					rect.width = -1;
					kittenVisualElement.uv = rect;
				}

				int yRandom = Mathf.RoundToInt(Random.value);
				Action<float> setY;
				if (yRandom == 0)
				{
					setY = v => kittenVisualElement.style.top = v;
					Rect rect = kittenVisualElement.uv;
					rect.height = -1;
					kittenVisualElement.uv = rect;
				}
				else
				{
					setY = v => kittenVisualElement.style.bottom = v;
					Rect rect = kittenVisualElement.uv;
					rect.height = 1;
					kittenVisualElement.uv = rect;
				}

				setX(0);
				setY(0);

				{
					//Interpolate the kitten to be visible
					float startInTime = Time.realtimeSinceStartup;
					while (Time.realtimeSinceStartup - startInTime < emergeTime)
					{
						float time = Time.realtimeSinceStartup - startInTime;
						float value = curve.Evaluate(time / emergeTime);
						setX(value * -w);
						setY(value * -h);
						yield return null;
					}

					setX(0);
					setY(0);
				}

//				while (true) //Use this code to debug with the kitten sticking around forever
//				{
//					yield return null;
//				}

				//WAIT FOR KITTEN TO LEAVE
				float hold = Random.Range(holdTimeMin, holdTimeMax);
				yield return new WaitForSeconds(hold);

				//KITTEN LEAVES
				#if VERBOSE_DEBUGGING
				Debug.Log("Leave");
				#endif


				{
					//Interpolate the kitten to be outside the border of the window
					float startOutTime = Time.realtimeSinceStartup;
					while (Time.realtimeSinceStartup - startOutTime < leaveTime)
					{
						float time = Time.realtimeSinceStartup - startOutTime;
						float value = 1 - curve.Evaluate(time / leaveTime);
						setX(value * -w);
						setY(value * -h);
						yield return null;
					}
				}

				kittenVisualElement.RemoveFromHierarchy();


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

		/// <summary>
		/// This struct exists so I can debug the random window, not for any other purpose.
		/// </summary>
		private struct RootWithName
		{
			public readonly string name;
			public readonly VisualElement root;

			public RootWithName(string n, VisualElement vE)
			{
				name = n;
				root = vE;
			}
		}

		static List<RootWithName> DiscoverAllRootContexts()
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
			if (!DebugRelevantError(visualTreeProperty, "Panel / BaseVisualElementPanel.visualTree", "Property"))
				return null;

			PropertyInfo nameProperty = panelType.GetProperty("name", BindingFlags.NonPublic | BindingFlags.Instance);
			if (!DebugRelevantError(nameProperty, "Panel.name", "Property"))
				return null;

			IEnumerator enumerator = (IEnumerator) GetPanelsIteratorMethod.Invoke(null, null);

			List<RootWithName> roots = new List<RootWithName>();
			//Get all panels' root VisualElements to inject content into
			while (enumerator.MoveNext())
			{
				object enumeratorCurrent = enumerator.Current;
				object panel = kvpValueProperty.GetValue(enumeratorCurrent);
				string name = (string) nameProperty.GetValue(panel);
				if (name.Equals("Toolbar")) continue; // Avoid adding kittens to the toolbar because you might struggle to turn off kittens :P
				if (!(visualTreeProperty.GetValue(panel) is VisualElement visualTreeRoot)) continue;
				roots.Add(new RootWithName(name, visualTreeRoot));
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