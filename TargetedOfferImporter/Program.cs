using System;
using System.ServiceProcess;


namespace TargetedOfferImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            
            
            Console.WriteLine("TargetedOfferImporter importer console about to begin, continue?");
            Console.ReadLine();
            
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - TargetedOfferImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new TargetedOfferService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
