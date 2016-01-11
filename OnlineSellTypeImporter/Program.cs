using System;
using System.ServiceProcess;

namespace OnlineSellTypeImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            Console.WriteLine("Online sell type importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - Online sell type importer console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new OnlineSellTypeService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
