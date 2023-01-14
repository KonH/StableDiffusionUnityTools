using UnityEditor;
using UnityEngine;

namespace StableDiffusionUnityTools.Editor {
	public sealed class StableDiffusionUserSettings : ScriptableObject {
		public const string SettingsPath = "Assets/StableDiffusionUnityTools/StableDiffusionUserSettings.asset";

		[SerializeField]
		internal string _command;

		[SerializeField]
		internal string _resultPath;

		[SerializeField]
		internal bool _verboseLogs;

		public static StableDiffusionUserSettings GetOrCreateSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<StableDiffusionUserSettings>(SettingsPath);
			if ( settings == null ) {
				settings = CreateInstance<StableDiffusionUserSettings>();
				AssetDatabase.CreateAsset(settings, SettingsPath);
				AssetDatabase.SaveAssets();
			}
			return settings;
		}

		public static SerializedObject GetSerializedSettings() {
			return new SerializedObject(GetOrCreateSettings());
		}
	}
}