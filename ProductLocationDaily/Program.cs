using System;
using System.ServiceProcess;


namespace ProductLocationDailyImporter
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("ProductLocationDailyImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - ProductLocationDailyImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ProductLocationDailyService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
