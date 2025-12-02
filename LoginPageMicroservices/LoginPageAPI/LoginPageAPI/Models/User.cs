
using System.ComponentModel.DataAnnotations;

namespace LoginPageAPI.Models; 

public class User 
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Username {  get; set; }
    [Required]
    public string PasswordHash { get; set; }
}