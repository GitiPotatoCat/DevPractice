using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReinsuranceApi.Models;


[Table("Treaties")]
public class Treaty
{
    [Key]
    public int TreatyId { get; set; }

    [Required, MaxLength(20)]
    public string TreatyCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string TreatyName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string CedingCompany { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Reinsurer { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string TreatyType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LineOfBusiness { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string Country { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumInsured { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PremiumAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal CommissionPct { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal RetentionPct { get; set; }

    public DateTime InceptionDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    [Required, MaxLength(20)]
    [Column("Treaty_Status")]
    public string Status { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string UnderwriterName { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}