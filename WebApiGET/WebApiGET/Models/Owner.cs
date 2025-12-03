
using System.ComponentModel.DataAnnotations;

namespace WebApiGET.Models;

public class Owner 
{
    [Key]
    public int OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string OrgName { get; set; } = string.Empty;
    public int ProdId { get; set; }
    public string ProdName { get; set; } = string.Empty;
}