using System;
using System.ServiceProcess;

namespace AssortmentImporter
{
    class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("Assortment importer console about to begin, continue?");
            Console.ReadLine();
            var importer = new Importer();
            importer.Start();
            Console.WriteLine("Debug - Assortment importer console running - press any key to stop");
            Console.ReadKey();
            importer.Stop();
#else
            ServiceBase[] ServicesToRun = new ServiceBase[] 
            { 
                new AssortmentService() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
