using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;
using Contracts;
using NHibernate;
using CanDoExternalTransfer.Domain;

namespace CanDoExternalTransfer
{
    public class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    var configuration = new Configuration();
                    configuration.Configure(@"C:\Users\Wojdan\Documents\Visual Studio 2010\Projects\CanDoExternalTransfer\CanDoExternalTransfer\hibernate.cfg.xml");
                    configuration.AddAssembly(typeof(TransferItem).Assembly);
                    _sessionFactory = configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}