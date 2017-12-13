using System;
using System.ServiceModel;

namespace BatchCoreService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            BatchCoreTest srv = new BatchCoreTest();
            Console.ReadLine();
        }
    }

    public class BatchCoreTest : IDisposable
    {
        //DAService service;
        ServiceHost serviceHost = null;

        public BatchCoreTest()
        {
            //service = new DAService();
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the CalculatorService type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(DAService));//MCF通讯

            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();
            //Console.ReadLine();
        }

        public void Dispose()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
