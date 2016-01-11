using System;
using System.ServiceProcess;

namespace RecipePELImporter
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("RecipePELImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - RecipePELImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new RecipePELImporterService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
