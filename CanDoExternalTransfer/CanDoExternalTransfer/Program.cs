using System;
using Contracts;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.Data.Entity;
using log4net;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using CanDoExternalTransfer.Domain;


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
            Console.WriteLine("Debug: " + logMessege);
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
        public static void LoadHibernateCfg()
        {
            var cfg = new Configuration();
            cfg.Configure(@"C:\users\wojdan\documents\visual studio 2010\Projects\CanDoExternalTransfer\CanDoExternalTransfer\hibernate.cfg.xml");

            try
            {
                cfg.AddAssembly(typeof(TransferItem).Assembly);
            }
            catch (Exception e) { Console.WriteLine(e); }
            try
            {
                new SchemaExport(cfg).Execute(true, true, false);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }


        static void Main(string[] args)
        {
            LoadHibernateCfg();


            //var item = mock.makeTransferItem(new Guid(), new Guid(), "1", "3", 200.0, true, "ok", DateTime.Now);
            //repo.Add(item);

            ExternalTransfer transferMoney = new ExternalTransfer();
            ServiceHost sh = new ServiceHost(transferMoney, new Uri[] { new Uri(Constants.CanDoExternalTransferURI) });

            LogHelper.Info("Application started.");

            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }

            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            sh.AddServiceEndpoint(typeof(ICanDoExternalTransfer), new NetTcpBinding(SecurityMode.None), Constants.CanDoExternalTransferURI);
            sh.Open();
            LogHelper.Info("Service ICanDoExternalTransfer started.");

            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(new NetTcpBinding(SecurityMode.None), Constants.ServiceRepositoryURI);
            IServiceRepository serviceRepository = cf.CreateChannel();

            Program.registerService(serviceRepository);


            transferMoney.serviceRepository = serviceRepository;
            transferMoney.database = new ORMTransferDatabase();
            //transferMoney.TransferMoney("1", "2", 200.0);
            //transferMoney.GetPreviousTransfers("1");

            //Debugging ORMDatabase
            MockTransferDatabase mock = new MockTransferDatabase();
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "1", "3", 200.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "2", "2", 270.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "3", "1", 250.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "1", "3", 200.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "2", "3", 300.0, false, "not enough money", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "3", "2", 100.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "1", "2", 500.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "2", "1", 700.0, false, "not enough money", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "3", "1", 400.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "1", "2", 300.0, true, "ok", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "2", "3", 200.0, false, "not enough money", DateTime.Now));
            transferMoney.database.SaveTransfer(mock.makeTransfer(new Guid(), new Guid(), "3", "1", 600.0, true, "ok", DateTime.Now));

            LogHelper.Debug("Liczba transferów z 1 = " + transferMoney.GetPreviousTransfersSentFrom("1").Count.ToString());
            LogHelper.Debug("Liczba transferów do 1 = " + transferMoney.GetPreviousTransfersSentTo("1").Count.ToString());
            LogHelper.Debug("Liczba transferów do/z 1 = " + transferMoney.GetPreviousTransfers("1").Count.ToString());
            //

            //transferMoney.database.GetTransfers("1");

            Console.ReadKey();
        }

        private static void registerService(IServiceRepository serviceRepository)
        {
            try
            {
                serviceRepository.registerService(Constants.ServiceName, Constants.CanDoExternalTransferURI);
                LogHelper.Info("Service ICanDoExternalTransfer registered in IServiceRepository.");
                LogHelper.Info("Starting timer to keep connection with IServiceRepository");
                var timer = new System.Threading.Timer(e => keepConnection(serviceRepository), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            }
            catch (Exception e)
            {
                LogHelper.Error("Failed registering service at IServiceRepository\nException message: " + e.Message);
            }
        }

        private static void keepConnection(IServiceRepository sr)
        {
            try
            {
                sr.isAlive(Constants.ServiceRepositoryURI);
                LogHelper.Info("Keeping connenection alive.");
            }
            catch (Exception e)
            {
                LogHelper.Error("Connection with IServiceRepository lost.\nException message: " + e.Message);
            }
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class ExternalTransfer : ICanDoExternalTransfer
    {
        public IServiceRepository serviceRepository;
        public IAccountRepository accountRepository;
        public ITransferDatabase database;

        public bool TransferMoney(string clientAccountNumber, string recieverAccountNumber, double amount)
        {
            string IAccountRepositoryAddress = serviceRepository.getServiceAddress("AccountRepository");
            ChannelFactory<IAccountRepository> cf = new ChannelFactory<IAccountRepository>(new NetTcpBinding(SecurityMode.None), IAccountRepositoryAddress);
            accountRepository = cf.CreateChannel();

            AccountDetails sender = accountRepository.GetAccountInformation(clientAccountNumber);
            AccountDetails reciever = accountRepository.GetAccountInformation(recieverAccountNumber);

            LogHelper.Info("Attempt to transfer [" + amount + "] from account [" + clientAccountNumber + "] to account [" + recieverAccountNumber + "]");
            LogHelper.Debug("Senders account balance:   " + sender.Money);
            LogHelper.Debug("Recievers account balance: " + reciever.Money);

            if (sender.Money < amount)
            {
                LogHelper.Error("Sender has too low balance to make transer");

                Transfer thisTransfer = database.makeTransfer(new Guid(), sender.Id, sender.AccountNumber, reciever.AccountNumber, amount, false, "not enough money", DateTime.Now);
                database.SaveTransfer(thisTransfer);

                return false;
            }
            else
            {
                LogHelper.Debug("Transfer can be made");

                reciever.Money += amount;
                sender.Money -= amount;

                LogHelper.Info("Updating sender & reciever account balances.");
                accountRepository.UpdateAccountInformation(reciever);
                accountRepository.UpdateAccountInformation(sender);
                LogHelper.Debug("Balances updated");

                sender = accountRepository.GetAccountInformation(clientAccountNumber);
                reciever = accountRepository.GetAccountInformation(recieverAccountNumber);
                LogHelper.Debug("(After transfer) Senders account balance:   " + sender.Money);
                LogHelper.Debug("(After transfer) Recievers account balance: " + reciever.Money);

                Transfer thisTransfer = database.makeTransfer(new Guid(), sender.Id, sender.AccountNumber, reciever.AccountNumber, amount, true, "ok", DateTime.Now);
                database.SaveTransfer(thisTransfer);

                return true;
            }
        }

        public List<Transfer> GetPreviousTransfers(string accountNumber)
        {
            return database.GetTransfers(accountNumber);
        }

        public List<Transfer> GetPreviousTransfersSentFrom(string accountNumber)
        {
            return database.GetTransfersSentFrom(accountNumber);
        }

        public List<Transfer> GetPreviousTransfersSentTo(string accountNumber)
        {
            return database.GetTransfersSentTo(accountNumber);
        }

        public List<Transfer> GetPreviousTransfersFromDate(DateTime startDate)
        {
            return database.GetTransfersFromDate(startDate);
        }

    }

    public interface ITransferDatabase
    {
        List<Transfer> GetTransfers(string accountNumber);
        List<Transfer> GetTransfersSentFrom(string accountNumber);
        List<Transfer> GetTransfersSentTo(string accountNumber);
        List<Transfer> GetTransfersFromDate(DateTime date);
        void SaveTransfer(Transfer transfer);
        Transfer makeTransfer(Guid ID, Guid clientID, string clientAccNo, string recieverAccNo, double amount, bool success, string desc, DateTime date);
        void ClearDatabase();
    }

    public class ORMTransferDatabase : ITransferDatabase
    {
        public Transfer TransferItemToTransfer(TransferItem item)
        {
            return new Transfer
            {
                ID=item.ID,
                clientID = item.clientID,
                clientAccountNumber = item.clientAccountNumber,
                recieverAccountNumber = item.recieverAccountNumber,
                amount = item.amount,
                wasSuccessful = item.wasSuccessful,
                description = item.description,
                date = item.date
            };
        }

        public TransferItem TransferToTransferItem(Transfer item)
        {
            return new TransferItem
            {
                ID = item.ID,
                clientID = item.clientID,
                clientAccountNumber = item.clientAccountNumber,
                recieverAccountNumber = item.recieverAccountNumber,
                amount = item.amount,
                wasSuccessful = item.wasSuccessful,
                description = item.description,
                date = item.date
            };
        }

        public List<Transfer> GetTransfers(string accountNumber)
        {
            TransferRepository repo = new TransferRepository();
            List<Transfer> results = new List<Transfer>();
            List<TransferItem> items = repo.GetAllTransfersForAccount(accountNumber);
            for (int i = 0; i < items.Count; i++)
            {
                results.Add(TransferItemToTransfer(items[i]));
            }
            return results;
        }

        public List<Transfer> GetTransfersSentFrom(string accountNumber)
        {
            TransferRepository repo = new TransferRepository();
            List<Transfer> results = new List<Transfer>();
            List<TransferItem> items = repo.GetTransfersSentFrom(accountNumber);
            for (int i = 0; i < items.Count; i++)
            {
                results.Add(TransferItemToTransfer(items[i]));
            }
            return results;
        }

        public List<Transfer> GetTransfersSentTo(string accountNumber)
        {
            TransferRepository repo = new TransferRepository();
            List<Transfer> results = new List<Transfer>();
            List<TransferItem> items = repo.GetTransfersSentTo(accountNumber);
            for (int i = 0; i < items.Count; i++)
            {
                results.Add(TransferItemToTransfer(items[i]));
            }
            return results;
        }

        public List<Transfer> GetTransfersFromDate(DateTime date)
        {
            TransferRepository repo = new TransferRepository();
            List<Transfer> results = new List<Transfer>();
            List<TransferItem> items = repo.GetTransfersFromDate(date);
            for (int i = 0; i < items.Count; i++)
            {
                results.Add(TransferItemToTransfer(items[i]));
            }
            return results;
        }

        public void SaveTransfer(Transfer transfer)
        {
            TransferRepository repo = new TransferRepository();
            repo.Add(TransferToTransferItem(transfer));
        }

        public Transfer makeTransfer(Guid ID, Guid clientID, string clientAccNo, string recieverAccNo, double amount, bool success, string desc, DateTime date)
        {
            return new Transfer
            {
                ID = ID,
                clientID = clientID,
                clientAccountNumber = recieverAccNo,
                recieverAccountNumber = recieverAccNo,
                amount = amount,
                wasSuccessful = success,
                description = desc,
                date = date
            };
        }

        public void ClearDatabase()
        {
            TransferRepository repo = new TransferRepository();
            repo.dropDatabase();
        }
    }
    public class MockTransferDatabase : ITransferDatabase
    {
        List<Transfer> mockedTransfers = new List<Transfer>();

        public MockTransferDatabase()
        {
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "1", "3", 200.0, true, "ok", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "2", "3", 300.0, false, "not enough money", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "3", "2", 100.0, true, "ok", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "1", "2", 500.0, true, "ok", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "2", "1", 700.0, false, "not enough money", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "3", "1", 400.0, true, "ok", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "1", "2", 300.0, true, "ok", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "2", "3", 200.0, false, "not enough money", DateTime.Now));
            mockedTransfers.Add(makeTransfer(new Guid(), new Guid(), "3", "1", 600.0, true, "ok", DateTime.Now));
        }

        public Transfer makeTransfer(Guid ID, Guid clientID, string clientAccNo, string recieverAccNo, double amount, bool success, string desc, DateTime date)
        {
            return new Transfer
            {
                ID = ID,
                clientID = clientID,
                clientAccountNumber = recieverAccNo,
                recieverAccountNumber = recieverAccNo,
                amount = amount,
                wasSuccessful = success,
                description = desc,
                date = date
            };
        }

        public TransferItem makeTransferItem(Guid ID, Guid clientID, string clientAccNo, string recieverAccNo, double amount, bool success, string desc, DateTime date)
        {
            return new TransferItem
            {
                ID = ID,
                clientID = clientID,
                clientAccountNumber = recieverAccNo,
                recieverAccountNumber = recieverAccNo,
                amount = amount,
                wasSuccessful = success,
                description = desc,
                date = date
            };
        }

        public List<Transfer> GetTransfers(string accountNumber)
        {
            List<Transfer> results = new List<Transfer>();
            for (int i = 0; i < mockedTransfers.Count; i++)
            {
                if (mockedTransfers[i].clientAccountNumber.Equals(accountNumber))
                {
                    results.Add(mockedTransfers[i]);
                }
            }
            return results;
        }

        public List<Transfer> GetTransfersSentFrom(string accountNumber)
        {
            List<Transfer> results = new List<Transfer>();
            for (int i = 0; i < mockedTransfers.Count; i++)
            {
                if (mockedTransfers[i].clientAccountNumber == accountNumber)
                {
                    results.Add(mockedTransfers[i]);
                }
            }
            return results;
        }

        public List<Transfer> GetTransfersSentTo(string accountNumber)
        {
            List<Transfer> results = new List<Transfer>();
            for (int i = 0; i < mockedTransfers.Count; i++)
            {
                if (mockedTransfers[i].recieverAccountNumber == accountNumber)
                {
                    results.Add(mockedTransfers[i]);
                }
            }
            return results;
        }

        public List<Transfer> GetTransfersFromDate(DateTime date)
        {
            List<Transfer> results = new List<Transfer>();
            for (int i = 0; i < mockedTransfers.Count; i++)
            {
                if (mockedTransfers[i].date >= date)
                {
                    results.Add(mockedTransfers[i]);
                }
            }
            return results;
        }

        public void SaveTransfer(Transfer transfer)
        {
            LogHelper.Debug("Adding new transfer. Number of transsfers: " + mockedTransfers.Count);
            mockedTransfers.Add(transfer);
            LogHelper.Debug("Added new transfer. Number of transsfers: " + mockedTransfers.Count);

        }

        public void ClearDatabase()
        {
            mockedTransfers.Clear();
        }
    }
}
