using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public static class Roles { public const string Organiser = "Organiser"; public const string Participant = "Participant"; }
public class UserAccount {
    public int UserId { get; set; }
    [Required, MaxLength(80)] public string FirstName { get; set; } = "";
    [Required, MaxLength(80)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(180)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required, MaxLength(20)] public string Role { get; set; } = Roles.Participant;
    public ICollection<RaceEvent> OrganisedEvents { get; set; } = new List<RaceEvent>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
