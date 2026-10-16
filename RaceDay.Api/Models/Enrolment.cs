using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public class Enrolment {
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public UserAccount? Participant { get; set; }
    public int EventId { get; set; }
    public RaceEvent? Event { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    [Required, MaxLength(20)] public string Status { get; set; } = "Registered";
    public RaceResult? Result { get; set; }
}
