using System.Runtime.Serialization;

namespace SoapDemo.Models
{
    [DataContract(Namespace = "http://soamdemo.dev/models")]
    public sealed class TransactionResult
    {
        [DataMember(Order = 1, IsRequired = true)]
        public bool IsSuccess { get; init; }

        [DataMember(Order = 2)]
        public string TransactionId { get; init; } = string.Empty;

        [DataMember(Order = 3)]
        public decimal UpdatedBalance { get; init; }

        [DataMember(Order = 4)]
        public string Message { get; init; } = string.Empty;

        [DataMember(Order = 5)]
        public DateTime ProcessedAt { get; init; }
    }
}
