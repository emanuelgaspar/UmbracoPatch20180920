using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoPatch20180920
{
	public class UmbracoPatch20180920
	{
		public string Backup_Folder { get; private set; }
		public FileInfo DLL35 { get; private set; }
		public FileInfo DLL40 { get; private set; }
		public FileInfo DLL45 { get; private set; }

		public void Run()
		{
			LoadDLLs();
			EnsureBackupFolder();

			var websites = LoadWebsites();

			while (true)
			{
				try
				{
					Console.WriteLine();

					var unpatched = websites.Where(p => !p.IsPatched).ToList();
					for (var i = 0; i < unpatched.Count; i++)
					{
						Console.WriteLine($"[{i:00}] {unpatched[i].Name}");
					}

					Console.Write("What website would you like to patch? Type its index: ");
					var index = int.Parse(Console.ReadLine());

					var target = unpatched[index];
					target.Patch();

					Console.WriteLine();
					Console.WriteLine("Press enter to patch another website...");
					Console.ReadLine();
				}
				catch (Exception x)
				{
					Console.WriteLine($"{x.GetType().FullName} : {x.Message} : {x.StackTrace}");
				}
			}
		}

		private void LoadDLLs()
		{
			var dllFolder = Environment.CurrentDirectory;
			do
			{
				DLL35 = new FileInfo(dllFolder + @"\ClientDependency.Core.1.9.7-net35\ClientDependency.Core.dll");
				if (!DLL35.Exists) Console.WriteLine($"Please put 3.5 DLL here: {DLL35.FullName}");

				DLL40 = new FileInfo(dllFolder + @"\ClientDependency.Core.1.9.7-net40\ClientDependency.Core.dll");
				if (!DLL40.Exists) Console.WriteLine($"Please put 4.0 DLL here: {DLL40.FullName}");

				DLL45 = new FileInfo(dllFolder + @"\ClientDependency.Core.1.9.7-net45\ClientDependency.Core.dll");
				if (!DLL45.Exists) Console.WriteLine($"Please put 4.5+ DLL here: {DLL45.FullName}");

				if (DLL35.Exists && DLL40.Exists && DLL45.Exists) break;
				Console.WriteLine("Fix any issues above and hit Enter to continue.");
				Console.ReadLine();
			}
			while (true);
		}

		private void EnsureBackupFolder()
		{
			var path = Path.Combine(Environment.CurrentDirectory, @"backups");
			var info = new DirectoryInfo(path);
			if (!info.Exists) info.Create();
			Backup_Folder = info.FullName;
		}

		private List<UmbracoWebsite> LoadWebsites()
		{
			Console.Write("Websites folder: ");
			var path = Console.ReadLine();
			Console.WriteLine("Listing all websites. This may take a few minutes.");
			var files = Tools.EnumerateFiles(path, "Web.config");
			var result = new List<UmbracoWebsite>();
			foreach (var file in files)
			{
				// ignore Web.config files special rule for Web.config
				var website = new UmbracoWebsite(file, path, this);
				if (!website.IsValid) continue;
				result.Add(website);
			}
			return result;
		}
	}
}