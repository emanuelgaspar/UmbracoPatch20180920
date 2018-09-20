using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UmbracoPatch20180920
{
	public class UmbracoWebsite
	{
		public FileInfo File { get; private set; }

		public string DotNetVersion { get; private set; }
		public DirectoryInfo Bin { get; private set; }
		public DirectoryInfo ClientDependencyTemp { get; private set; }
		public UmbracoPatch20180920 Patcher { get; private set; }

		public bool IsValid => DotNetVersion != null && Bin != null;

		public string Name { get; private set; }
		public DateTime? TimePatched { get; private set; }

		public bool IsPatched => TimePatched != null;

		public UmbracoWebsite(FileInfo file, string parentPath, UmbracoPatch20180920 patcher)
		{
			this.File = file;
			this.Patcher = patcher;

			var text = System.IO.File.ReadAllText(file.FullName);
			if (!text.Contains("<umbracoConfiguration")) return; // not an Umbraco website

			var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			ClientDependencyTemp = LoadClientDependencyTemp(file);

			this.Bin = file.Directory
							.GetDirectories("bin", SearchOption.TopDirectoryOnly)
							.FirstOrDefault();
			
			DotNetVersion = GetDotNetVersion(lines);
			
			this.Name = Tools.GetCleanDirectoryName(file, parentPath, @"\Web.config");
		}

		internal void Patch()
		{
			var done = ReplaceDll();
			if (!done) return;
			
			ClearClientDependencyTemp();

			TimePatched = DateTime.Now;
		}

		private bool ReplaceDll()
		{
			FileInfo dll = null;
			if (DotNetVersion.StartsWith("3.5")) dll = Patcher.DLL35;
			if (DotNetVersion.StartsWith("4.0")) dll = Patcher.DLL40;
			if (DotNetVersion.StartsWith("4.5")) dll = Patcher.DLL45;

			if (dll == null)
			{
				Console.WriteLine($"Warning: unknown version {DotNetVersion}");
				Console.WriteLine("Resuming with default (.Net 4.5) . Press Enter to confirm.");
				Console.ReadLine();
				dll = Patcher.DLL45;
			}

			var dllPath = Path.Combine(Bin.FullName, "ClientDependency.Core.dll");

			var backupDLL = Path.Combine(Patcher.Backup_Folder, $"ClientDependency.Core-{Name.Replace("\\", "-")}.dll");
			var originalDLL = new FileInfo(dllPath).CopyTo(backupDLL, true);

			dll.CopyTo(dllPath, true);

			Console.WriteLine($"DLL version {DotNetVersion} replaced.");
			return true;
		}

		private void ClearClientDependencyTemp()
		{
			if (ClientDependencyTemp == null)
			{
				Console.WriteLine("ClientDependencyTemp folder not found");
				return;
			}
			foreach (FileInfo file in ClientDependencyTemp.EnumerateFiles())
			{
				file.Delete();
			}
			foreach (DirectoryInfo dir in ClientDependencyTemp.EnumerateDirectories())
			{
				dir.Delete(true);
			}
			Console.WriteLine($"ClientDependencyTemp has {ClientDependencyTemp.GetFileSystemInfos()?.Count()} items");
		}

		private string GetDotNetVersion(IEnumerable<string> lines)
		{
			var version = lines.FirstOrDefault(p => p.IndexOf("targetFramework=", StringComparison.CurrentCultureIgnoreCase) >= 0);
			if (version == null) return null;

			return version
					.Split(new string[] { "targetFramework=\"" }, StringSplitOptions.RemoveEmptyEntries)
					.Last()
					.Split('"')
					.First();
		}

		private DirectoryInfo LoadClientDependencyTemp(FileInfo file)
		{
			var appData = file.Directory
							.GetDirectories("App_Data", SearchOption.TopDirectoryOnly)
							.FirstOrDefault();
			if (appData == null) return null;

			//all files in ~/ App_Data / ClientDependency or ~/ App_Data / Temp / ClientDependency
			var temp = appData.GetDirectories("ClientDependency", SearchOption.TopDirectoryOnly).FirstOrDefault();
			if (temp != null) return temp;

			var innerTemp = appData.GetDirectories("Temp", SearchOption.TopDirectoryOnly).FirstOrDefault();
			if (innerTemp == null) return null;

			return innerTemp.GetDirectories("ClientDependency", SearchOption.TopDirectoryOnly).FirstOrDefault();
		}
	}
}