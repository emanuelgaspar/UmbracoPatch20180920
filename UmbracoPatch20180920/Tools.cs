using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UmbracoPatch20180920
{
	public static class Tools
	{
		internal static IEnumerable<FileInfo> EnumerateFiles(string path, string fileName)
		{
			var folder = new DirectoryInfo(path);
			if (!folder.Exists)
			{
				Console.WriteLine("Folder does not exist.");
				yield break;
			}

			var files = folder.EnumerateFiles(fileName, SearchOption.AllDirectories);
			foreach (var file in files)
			{
				yield return file;
			}
		}

		internal static string GetCleanDirectoryName(FileInfo file, string parentPath, string relativePath)
		{
			return file.FullName
									.Replace(parentPath, string.Empty)
									.ReplaceString(parentPath, string.Empty, StringComparison.CurrentCultureIgnoreCase)
									.ReplaceString(relativePath, string.Empty, StringComparison.CurrentCultureIgnoreCase)
									.TrimStart('\\');
		}

		public static string ReplaceString(this string originalString,
			string oldValue, string newValue, StringComparison comparisonType)
		{
			int startIndex = 0;
			while (true)
			{
				startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
				if (startIndex == -1)
					break;

				originalString = originalString.Substring(0, startIndex) + newValue +
					originalString.Substring(startIndex + oldValue.Length);

				startIndex += newValue.Length;
			}

			return originalString;
		}
	}
}