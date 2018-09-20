using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoPatch20180920
{
	class Program
	{
		static void Main(string[] args)
		{
			var patch = new UmbracoPatch20180920();
			patch.Run();
			Console.WriteLine("Press Enter to close...");
			Console.ReadLine();
		}
	}
}
