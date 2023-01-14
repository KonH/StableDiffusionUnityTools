using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace StableDiffusionUnityTools.Editor {
	sealed class AssetGenerator {
		readonly Dictionary<Guid, Process> _pendingOperations = new Dictionary<Guid, Process>();

		public void StartGeneration(GenerationSettings generationSettings, Action<string> onFileCreated) {
			ValidateSettings(generationSettings);
			var userSettings = StableDiffusionUserSettings.GetOrCreateSettings();
			var rawCommand = userSettings._command;
			var (cmd, rawArgs) = ExtractCommandAndArgs(rawCommand);
			var guid = Guid.NewGuid();
			var args = PrepareArgs(rawArgs, generationSettings, guid); 
			RunCommand(
				userSettings, generationSettings,
				guid, cmd, args, onFileCreated);
		}

		void ValidateSettings(GenerationSettings settings) {
			Assert.IsTrue(!string.IsNullOrWhiteSpace(settings.Prompt), "Prompt is empty");
			Assert.IsTrue(Directory.Exists(settings.Directory), "Directory is not exist");
			Assert.IsTrue(settings.Width > 0, "Width is invalid");
			Assert.IsTrue(settings.Height > 0, "Height is invalid");
		}

		(string, string) ExtractCommandAndArgs(string rawCommand) {
			Assert.IsTrue(!string.IsNullOrWhiteSpace(rawCommand), "Command is not set up");
			var singleLineCommand = rawCommand.Replace('\n', ' ');
			var commandParts = singleLineCommand.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			Assert.IsTrue(commandParts.Length > 1, "Command is invalid");
			var cmd = commandParts[0];
			var args = string.Join(" ", commandParts.Skip(1).ToArray());
			return (cmd, args);
		}

		string PrepareArgs(string rawArgs, GenerationSettings settings, Guid guid) {
			var finalArgs = rawArgs
				.ReplaceRequiredArgument("$PROMPT", settings.Prompt)
				.ReplaceRequiredArgument("$GUID", guid.ToString())
				.ReplaceRequiredArgument("$WIDTH", settings.Width.ToString())
				.ReplaceRequiredArgument("$HEIGHT", settings.Height.ToString());
			return finalArgs;
		}

		void RunCommand(
			StableDiffusionUserSettings userSettings, GenerationSettings generationSettings,
			Guid guid, string cmd, string args, Action<string> onFileCreated) {
			Debug.Log($"[{guid}] Run command '{cmd}' with arguments '{args}'");
			var sw = Stopwatch.StartNew();
            var proc = CreateProcess(cmd, args);
            _pendingOperations[guid] = proc;
            
            proc.Exited += (sender, eventArgs) =>
				OnProcessExited(userSettings, generationSettings, guid, onFileCreated, sw);
            
            if ( userSettings._verboseLogs ) {
	            Debug.Log($"[{guid}] Process created");
	            proc.OutputDataReceived += (sender, eventArgs) => 
		            Debug.Log($"[{guid}] Output data: {eventArgs.Data}");
	            proc.ErrorDataReceived += (sender, eventArgs) =>
		            Debug.Log($"[{guid}] Error data: {eventArgs.Data}");
	            proc.Disposed += (sender, eventArgs) => 
		            Debug.Log($"[{guid}] Process disposed");
            }

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            if ( userSettings._verboseLogs ) {
            	Debug.Log($"[{guid}] Process started");
            }
		}

		private void OnProcessExited(
			StableDiffusionUserSettings userSettings, GenerationSettings generationSettings,
			Guid guid, Action<string> onFileCreated, Stopwatch sw) {
			sw.Stop();
			var proc = _pendingOperations[guid];
			Debug.Log($"[{guid}] Process finished for {sw.Elapsed.TotalSeconds:F} seconds (exit code = {proc.ExitCode})");
			proc.Dispose();
			_pendingOperations.Remove(guid);
			var pathToLookup = $"{userSettings._resultPath}/{guid}/samples";
			var files = Directory.GetFiles(pathToLookup, "*.png");
			Debug.Log($"[{guid}] Found {files.Length} files at '{pathToLookup}'");
			var fileName = !string.IsNullOrWhiteSpace(generationSettings.Name)
				? generationSettings.Name
				: guid.ToString();
			foreach ( var file in files ) {
				var targetFileName = $"{fileName}_{Path.GetFileName(file)}";
				var targetFilePath = $"{generationSettings.Directory}/{targetFileName}";
				File.Copy(file, targetFilePath);
				try {
					onFileCreated(targetFilePath);
				} catch ( Exception e ) {
					Debug.LogError($"[{guid}] {e}");
				}

				Debug.Log($"[{guid}] File is ready at {targetFilePath}");
			}
		}

		Process CreateProcess(string cmd, string args) {
			var startInfo = new ProcessStartInfo {
				FileName = cmd,
				Arguments = args,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};
			var proc = new Process {
				EnableRaisingEvents = true,
				StartInfo = startInfo,
			};
			return proc;
		}
	}
}