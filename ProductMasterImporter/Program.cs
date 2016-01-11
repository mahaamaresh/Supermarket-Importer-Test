using System;
using System.ServiceProcess;

namespace ProductMasterImporter
{
    public class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("ProductMasterImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - ProductMasterImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ProductMasterImporterService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
