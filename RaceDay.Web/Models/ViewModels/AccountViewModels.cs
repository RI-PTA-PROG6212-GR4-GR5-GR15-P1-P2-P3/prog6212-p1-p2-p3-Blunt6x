using System.ComponentModel.DataAnnotations;
namespace RaceDay.Web.Models.ViewModels;

public class LoginViewModel {
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Enter your password."), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel {
    [Required(ErrorMessage = "Enter your first name."), StringLength(80), Display(Name = "First name")]
    public string FirstName { get; set; } = "";
    [Required(ErrorMessage = "Enter your last name."), StringLength(80), Display(Name = "Last name")]
    public string LastName { get; set; } = "";
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Choose a password."), MinLength(8, ErrorMessage = "Your password must be at least 8 characters long."), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    [Required(ErrorMessage = "Confirm your password."), DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "The passwords don't match."), Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
    [Required(ErrorMessage = "Choose how you'll use RaceDay.")]
    public string Role { get; set; } = "Participant";
}

public class ProfileViewModel {
    [Required(ErrorMessage = "Enter your first name."), StringLength(80), Display(Name = "First name")]
    public string FirstName { get; set; } = "";
    [Required(ErrorMessage = "Enter your last name."), StringLength(80), Display(Name = "Last name")]
    public string LastName { get; set; } = "";
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
}
