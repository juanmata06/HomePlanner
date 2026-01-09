using System;
using System.ComponentModel.DataAnnotations;

namespace HomePlanner.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
    
    public string? Role { get; set; }
}
