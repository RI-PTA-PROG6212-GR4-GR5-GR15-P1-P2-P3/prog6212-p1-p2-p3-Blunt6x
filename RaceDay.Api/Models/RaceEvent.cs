using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public class RaceEvent {
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public UserAccount? Organiser { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = "";
    [MaxLength(1000)] public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    [Required, MaxLength(180)] public string Location { get; set; } = "";
    [Range(0.1, 1000)] public decimal Distance { get; set; }
    [Required, MaxLength(10)] public string EventType { get; set; } = "Run";
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
