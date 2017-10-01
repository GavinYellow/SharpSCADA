using System.ServiceProcess;

namespace BatchCoreService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new BatchCoreService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
