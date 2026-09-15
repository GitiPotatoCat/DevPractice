using CoreWCF;
using SoapDemo.Models;

namespace SoapDemo.Contracts
{
    [ServiceContract(Namespace = "http://soamdemo.dev/banking")]
    public interface IBankAccountService
    {
        [OperationContract]
        AccountInfo GetAccountInfo(string accountNumber);

        [OperationContract]
        TransactionResult Deposit(string accountNumber, decimal amount);

        [OperationContract]
        TransactionResult Withdraw(string accountNumber, decimal amount);
    }
}
