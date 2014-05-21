using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;


namespace Contracts
{
    [ServiceContract]
    public interface ICanDoExternalTransfer
    {
        [OperationContract]
        bool TransferMoney(Guid clientID, Guid externalClientID, double amount);

        [OperationContract]
        Transfer[] GetPreviousTransfers();
    }

    [DataContract]
    public class Transfer
    {
        [DataMember]
        public Guid clientID { get; set; }
        
        [DataMember]
        public Guid externalClientID {get; set;}

        [DataMember]
        public double amount { get; set; }

        [DataMember]
        public bool wasSuccessful { get; set; }

    }

}