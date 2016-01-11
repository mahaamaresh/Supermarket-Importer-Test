using System;
using System.ServiceProcess;

namespace BonusPointsImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            Console.WriteLine("Bonus Points importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - Bonus Points importer console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new BonusPointsService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
