using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using Contracts;

namespace ServiceRepository
{
    class Program
    {
        private const int MaxBufferSize = 10000000;
        private const int MaxBufferPoolSize = 10000000;
        private const int MaxReceivedMessageSize = 10000000;
        private const int ReceiveTimeout = 10000000;
        private const string ServiceURI = "net.tcp://localhost:11900/IServiceRepository";

        static void Main(string[] args)
        {
            ServiceRepository serviceRepository = new ServiceRepository();

            ServiceHost sh = new ServiceHost(serviceRepository, new Uri[] { new Uri(ServiceURI) });

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();
           
            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            NetTcpBinding serviceRepositoryBinding = new NetTcpBinding(SecurityMode.None);
            serviceRepositoryBinding.MaxBufferSize = MaxBufferSize;
            serviceRepositoryBinding.MaxBufferPoolSize = MaxBufferPoolSize;
            serviceRepositoryBinding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            //serviceRepositoryBinding.ReceiveTimeout = ReceiveTimeout;
            //serviceRepositoryBinding.SendTimeout = new System.TimeSpan(1, 0, 0);

            sh.AddServiceEndpoint(typeof(IServiceRepository), serviceRepositoryBinding, ServiceURI);


            sh.Open();
            Console.WriteLine("Serwis uruchomiony...");
            Console.ReadLine();
        }
    }

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    public class ServiceRepository : IServiceRepository
    {
        Dictionary<string, string> services = new Dictionary<string, string>();

        public void registerService(string serviceName, string serviceAddress)
        {
            services.Add(serviceName, serviceAddress);
            Console.WriteLine("Dodano serwis: {0} {1}", serviceName, serviceAddress);

            Console.WriteLine("Aktualna lista serwisów:");
            foreach (KeyValuePair<string, string> service in services)
            {
                Console.WriteLine("Key: {0}, Value: {1}",
                service.Key, service.Value);
            }
        }

        public void unregisterService(string serviceName)
        {
            services.Remove(serviceName);
        }

        public string getServiceAddress(string serviceName)
        {
            return services[serviceName];
        }

        public void isAlive(string serviceName)
        {
            Console.WriteLine(serviceName + " zgłasza obecność.");
        }

    }
}

