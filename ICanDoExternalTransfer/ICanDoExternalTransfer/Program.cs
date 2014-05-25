using System;
using Contracts;
using System.ServiceModel;
using System.ServiceModel.Description;
using log4net;

namespace CanDoExternalTransfer
{

    class Constants
    {
        public const string ServiceName = "CanDoExternalTransfer";
        public const string CanDoExternalTransferURI = "net.tcp://localhost:11910/ICanDoExternalTransfer";
        public const string ServiceRepositoryURI = "net.tcp://localhost:11900/IServiceRepository";
    }

    class LogHelper
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void Debug(string logMessege)
        {
            log.Debug(logMessege);
            Console.WriteLine("Debug: "+logMessege);
        }

        public static void Error(string logMessege)
        {
            log.Error(logMessege);
            Console.WriteLine("Error: " + logMessege);
        }

        public static void Info(string logMessege)
        {
            log.Info(logMessege);
            Console.WriteLine("Info: " + logMessege);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ExternalTransfer transferMoney = new ExternalTransfer();
            ServiceHost sh = new ServiceHost(transferMoney, new Uri[] { new Uri(Constants.CanDoExternalTransferURI) });

            LogHelper.Info("Application started");

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

            try
            {
                serviceRepository.registerService(Constants.ServiceName, Constants.CanDoExternalTransferURI); 
                var timer = new System.Threading.Timer(e => keepConnection(serviceRepository), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message);
            }
            
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
