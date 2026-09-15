using SoapDemo.Contracts;
using SoapDemo.Models;

namespace SoapDemo.Services
{
    public class CalculatorService : ICalculatorService
    {
        private readonly ILogger _logger;

        public CalculatorService(ILogger<CalculatorService> logger)
        {
            _logger = logger;
        }


        #region Interface implementation methods
        public double Addition(double a, double b)
        {
            _logger.LogInformation("Addition called: {A} + {B}", a, b);
            return a + b;
        }
        
        public double Subtraction(double a, double b)
        {
            _logger.LogInformation("Subtraction called: {A} - {B}", a, b);
            return a - b;
        }

        public OperationResult Multiplication(double a, double b)
        {
            var result = a * b;
            _logger.LogInformation("Multiplication called: {A} * {B}", a, b);
            return new OperationResult
            {
                Result = result, 
                Operation = $"{a} x {b}", 
                TimeStamp = DateTime.UtcNow
            };
        }
        #endregion
    }
}
