using CoreWCF;
using SoapDemo.Models;

namespace SoapDemo.Contracts
{
    [ServiceContract(Namespace = "https://soamdemo.dev/calculator")]
    public interface ICalculatorService
    {
        [OperationContract]
        double Addition(double a, double b);

        [OperationContract]
        double Subtraction(double a, double b);

        [OperationContract]
        OperationResult Multiplication(double a, double b);
    }
}
