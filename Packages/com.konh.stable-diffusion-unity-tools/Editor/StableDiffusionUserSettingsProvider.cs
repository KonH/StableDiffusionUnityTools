using System.IO;
using StableDiffusionUnityTools.Editor;
using UnityEditor;
using UnityEngine.UIElements;

sealed class StableDiffusionUserSettingsProvider : SettingsProvider {
	SerializedObject _customSettings;

	public StableDiffusionUserSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
		: base(path, scope) {}

	public static bool IsSettingsAvailable() {
		return File.Exists(StableDiffusionUserSettings.SettingsPath);
	}

	public override void OnActivate(string searchContext, VisualElement rootElement) {
		_customSettings = StableDiffusionUserSettings.GetSerializedSettings();
	}

	public override void OnGUI(string searchContext) {
		StableDiffusionUserSettingsDrawer.OnGUI(_customSettings, searchContext);
	}

	[SettingsProvider]
	public static SettingsProvider CreateMyCustomSettingsProvider() {
		if ( IsSettingsAvailable() ) {
			var provider = new StableDiffusionUserSettingsProvider("Project/Stable Diffusion", SettingsScope.Project) {
				keywords = GetSearchKeywordsFromGUIContentProperties<StableDiffusionUserSettingsStyles>()
			};
			return provider;
		}
		return null;
	}
}