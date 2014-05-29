using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanDoExternalTransfer.Domain
{
    public class TransferItem
    {
        public virtual Guid ID { get; set; }
        public virtual Guid clientID { get; set; }
        public virtual string clientAccountNumber { get; set; }
        public virtual string recieverAccountNumber { get; set; }
        public virtual double amount { get; set; }
        public virtual bool wasSuccessful { get; set; }
        public virtual string description { get; set; }
        public virtual DateTime date { get; set; }

    }
}
