using System;
using System.ServiceProcess;

namespace RelevantSpecialsImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            Console.WriteLine("RelevantSpecialsImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - RelevantSpecialsImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new RelevantSpecialsService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
