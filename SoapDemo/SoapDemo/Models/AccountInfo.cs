using System.Runtime.Serialization;

namespace SoapDemo.Models
{
    [DataContract(Namespace = "http://soamdemo.dev/models")]
    public sealed class AccountInfo
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string AccountNumber { get; init; } = string.Empty;

        [DataMember(Order = 2, IsRequired = true)]
        public string HolderName {  get; init; } = string.Empty;

        [DataMember(Order = 3)]
        public decimal Balance { get; init; }

        [DataMember(Order = 4)]
        public string Currency { get; init; } = string.Empty;

        [DataMember(Order = 5)]
        public string AccountType { get; init; } = string.Empty;
    }
}
