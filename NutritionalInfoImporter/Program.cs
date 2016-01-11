using System.ServiceProcess;
using System;

namespace NutritionalInfoImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG            
            Console.WriteLine("Nutritional Info importer console about to begin, continue?");
            Console.ReadLine();
            Importer importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - Nutritional Info importer console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();            
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new NutritionalInfoService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
