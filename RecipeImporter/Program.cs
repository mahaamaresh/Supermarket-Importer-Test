using System;
using System.ServiceProcess;

namespace RecipeImporter
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("RecipeImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - RecipeImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new RecipeImporterService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
