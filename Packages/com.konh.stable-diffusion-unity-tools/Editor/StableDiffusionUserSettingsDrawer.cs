using StableDiffusionUnityTools.Editor;
using UnityEditor;

static class StableDiffusionUserSettingsDrawer {
	public static void OnGUI(SerializedObject settings, string searchContext) {
		EditorGUILayout.LabelField("Command:");
		var commandProperty = settings.FindProperty(nameof(StableDiffusionUserSettings._command));
		commandProperty.stringValue = EditorGUILayout.TextArea(commandProperty.stringValue);
		EditorGUILayout.PropertyField(
			settings.FindProperty(nameof(StableDiffusionUserSettings._resultPath)),
			StableDiffusionUserSettingsStyles.ResultPath);
		EditorGUILayout.PropertyField(
			settings.FindProperty(nameof(StableDiffusionUserSettings._verboseLogs)),
			StableDiffusionUserSettingsStyles.VerboseLogs);
		settings.ApplyModifiedProperties();
	}
}