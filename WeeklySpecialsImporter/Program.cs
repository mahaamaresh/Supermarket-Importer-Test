using System;
using System.ServiceProcess;

namespace WeeklySpecialsImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG

            Console.WriteLine("WeeklySpecialsImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - WeeklySpecialsImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new WeeklySpecialsService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
