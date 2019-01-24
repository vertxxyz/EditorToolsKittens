using System.Collections.Generic;
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

		void OnEnable()
		{
			_iconContent = new GUIContent(LoadTextureFromString(KittensImages.Icon), "🐱");
		}

		private static Texture2D LoadTextureFromString(string s)
		{
			byte[] rawTextureData = System.Convert.FromBase64String(s);
			Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			tex.LoadImage(rawTextureData);
			return tex;
		}

		public override void OnToolGUI(EditorWindow window)
		{
			
		}

		List<VisualElement> DiscoverAllWindowContexts()
		{
			return null;
		}

		private void OnDisable()
		{
			DestroyImmediate(_iconContent.image);
		}
	}
}