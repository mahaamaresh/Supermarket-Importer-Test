using System;
using System.ServiceProcess;

namespace ProductWebCategoryImporter
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("ProductWebCategoryImporter importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - ProductWebCategoryImporter console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ProductWebCategoryService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
