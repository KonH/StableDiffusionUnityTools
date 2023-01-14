using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StableDiffusionUnityTools.Editor {
	sealed class AssetsPromptWindow : EditorWindow {
		readonly AssetGenerator _generator = new AssetGenerator();
		readonly HashSet<string> _pendingFiles = new HashSet<string>();

		SerializedObject _serializedObject;
		
		[SerializeField] 
		string _prompt;

		[SerializeField]
		string _name;
		
		[SerializeField]
		string _directory;

		[SerializeField]
		int _width;

		[SerializeField]
		int _height;

		public void OnGUI() {
			TryImportFiles();
			
			_serializedObject ??= new SerializedObject(this);

			var promptProperty = _serializedObject.FindProperty(nameof(_prompt));
			EditorGUILayout.LabelField("Prompt");
			promptProperty.stringValue = EditorGUILayout.TextArea(promptProperty.stringValue);
			EditorGUILayout.PropertyField(
				_serializedObject.FindProperty(nameof(_name)), new GUIContent("Name"));
			EditorGUILayout.PropertyField(
				_serializedObject.FindProperty(nameof(_directory)), new GUIContent("Directory"));
			EditorGUILayout.PropertyField(
				_serializedObject.FindProperty(nameof(_width)), new GUIContent("Width"));
			EditorGUILayout.PropertyField(
				_serializedObject.FindProperty(nameof(_height)), new GUIContent("Height"));
			_serializedObject.ApplyModifiedProperties();
			
			if ( GUILayout.Button("Generate") ) {
				var settings = new GenerationSettings {
					Prompt = _prompt,
					Name = _name,
					Directory = _directory,
					Width = _width,
					Height = _height,
				};
				_generator.StartGeneration(settings, OnFileCreated);
			}
		}

		void OnFileCreated(string path) {
			_pendingFiles.Add(path);
		}

		void TryImportFiles() {
			if ( _pendingFiles.Count == 0 ) {
				return;
			}
			foreach ( var x in _pendingFiles ) {
				AssetDatabase.ImportAsset(x);
			}
			_pendingFiles.Clear();
		}
	}
}