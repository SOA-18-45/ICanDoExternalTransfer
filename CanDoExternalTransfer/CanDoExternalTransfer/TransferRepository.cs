using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Contracts;
using CanDoExternalTransfer.Domain;

namespace CanDoExternalTransfer
{
    class TransferRepository
    {
        public void Add(TransferItem newTransaction)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                using (ITransaction dbtransaction = session.BeginTransaction())
                {
                    session.Save(newTransaction);
                    dbtransaction.Commit();
                }
            }
        }

        public List<TransferItem> GetAllTransfersForAccount(string accountNumber)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<TransferItem> results = new List<TransferItem>();
                results = (List<TransferItem>)session.QueryOver<TransferItem>().Where(x => (x.clientAccountNumber == accountNumber || x.recieverAccountNumber == accountNumber)).List<TransferItem>();
                return results;
            }
        }

        public List<TransferItem> GetTransfersSentFrom(string accountNumber)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<TransferItem> results = new List<TransferItem>();
                results = (List<TransferItem>)session.QueryOver<TransferItem>().Where(x => x.clientAccountNumber == accountNumber).List<TransferItem>();
                return results;
            }
        }

        public List<TransferItem> GetTransfersSentTo(string accountNumber)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<TransferItem> results = new List<TransferItem>();
                results = (List<TransferItem>)session.QueryOver<TransferItem>().Where(x => x.recieverAccountNumber == accountNumber).List<TransferItem>();
                return results;
            }
        }

        public List<TransferItem> GetTransfersFromDate(DateTime startDate)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                List<TransferItem> results = new List<TransferItem>();
                results = (List<TransferItem>)session.QueryOver<TransferItem>().Where(x => x.date >= startDate).List<TransferItem>();
                return results;
            }
        }

        public void dropDatabase()
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                using (ITransaction dbtransaction = session.BeginTransaction())
                {
                    session.Delete("*");
                    dbtransaction.Commit();
                }
            }
        }
    }
}
