using System.ComponentModel.DataAnnotations;
public class User
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}