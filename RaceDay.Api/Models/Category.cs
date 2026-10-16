using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public class Category {
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public RaceEvent? Event { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    [MaxLength(250)] public string? Description { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
