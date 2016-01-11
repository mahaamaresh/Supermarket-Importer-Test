using System;
using System.ServiceProcess;

namespace HealthWellbeingImporter
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("HealthWellbeingImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - HealthWellbeingImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new HealthWellbeingService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif

        }
    }
}
