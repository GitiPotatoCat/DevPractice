using System.Runtime.Serialization;

namespace SoapDemo.Models
{
    [DataContract(Namespace = "http://soamdemo.dev/models")]
    public sealed class OperationResult
    {
        [DataMember(Order = 1, IsRequired = true)]
        public double Result { get; init; }

        [DataMember(Order = 2)]
        public string Operation { get; init; } = string.Empty;

        [DataMember(Order = 3)]
        public DateTime TimeStamp { get; init; }
    }
}
