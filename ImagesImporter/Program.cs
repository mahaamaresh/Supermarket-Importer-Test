using System;
using System.ServiceProcess;

namespace ImagesImporter
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG

            Console.WriteLine("ImagesImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - ImagesImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ImagesImporterService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif

        }
    }
}
