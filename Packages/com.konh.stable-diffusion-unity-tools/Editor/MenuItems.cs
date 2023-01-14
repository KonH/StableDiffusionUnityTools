using System.IO;
using UnityEditor;

namespace StableDiffusionUnityTools.Editor {
	public static class MenuItems {
		[MenuItem("SD Tools/Setup")]
		public static void Setup() {
			Directory.CreateDirectory("Assets/StableDiffusionUnityTools");
			StableDiffusionUserSettings.GetOrCreateSettings();
		}

		[MenuItem("SD Tools/Generate Image")]
		public static void GenerateImage() {
			EditorWindow.GetWindow<AssetsPromptWindow>("Image Generation");
		}
	}
}