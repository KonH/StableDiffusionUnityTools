using UnityEngine.Assertions;

namespace StableDiffusionUnityTools.Editor {
	static class ArgumentExtensions {
		public static string ReplaceRequiredArgument(this string args, string key, string replacement) {
			Assert.IsTrue(args.Contains(key), $"Required argument key '{key}' is not found");
			return args.Replace(key, replacement);
		}
	}
}