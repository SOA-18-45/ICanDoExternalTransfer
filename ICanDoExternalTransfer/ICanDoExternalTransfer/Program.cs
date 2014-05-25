using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using Contracts;

namespace CanDoExternalTransfer
{

    class Constants
    {
        public const string ServiceName = "CanDoExternalTransfer";
        public const string CanDoExternalTransferURI = "net.tcp://localhost:11910/ICanDoExternalTransfer";
        public const string ServiceRepositoryURI = "net.tcp://localhost:11900/IServiceRepository";
    }

    class Program
    {
        static void Main(string[] args)
        {

            ExternalTransfer transferMoney = new ExternalTransfer();
            ServiceHost sh = new ServiceHost(transferMoney, new Uri[] { new Uri(Constants.CanDoExternalTransferURI) });

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            sh.AddServiceEndpoint(typeof(ICanDoExternalTransfer), new NetTcpBinding(SecurityMode.None), Constants.CanDoExternalTransferURI);
            sh.Open();

            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), Constants.ServiceRepositoryURI);
            IServiceRepository serviceRepository = cf.CreateChannel();

            serviceRepository.registerService(Constants.ServiceName, Constants.CanDoExternalTransferURI);

            var timer = new System.Threading.Timer(e => keepConnection(serviceRepository), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            Console.ReadLine();
        }

        private static void keepConnection(IServiceRepository sr)
        {
            sr.isAlive(Constants.ServiceRepositoryURI);
            Console.WriteLine("I am alive!");
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class ExternalTransfer : ICanDoExternalTransfer
    {

        public bool TransferMoney(Guid clientID, Guid externalClientID, double amount)
        {
            return true;
        }

        public Transfer[] GetPreviousTransfers()
        {
            return null;
        }

    }
}
