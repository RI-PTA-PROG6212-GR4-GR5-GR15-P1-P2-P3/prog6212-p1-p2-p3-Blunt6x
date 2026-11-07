namespace RaceDay.Web.Models.Api;

// Shapes returned by / sent to the RaceDay.Api. The MVC app holds no database access and no business rules.

public class LoginResponse {
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
}

public class ProfileDto {
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
}

public class EventDto {
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerUrl { get; set; }
    // Only populated by /api/events/mine
    public int EnrolmentCount { get; set; }
    public int CategoryCount { get; set; }
    public int ResultCount { get; set; }
    public bool IsUpcoming => Date.Date >= DateTime.Today;
}

public class CategoryDto {
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
}

public class EventRefDto {
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerUrl { get; set; }
}

public class CategoryRefDto {
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
}

public class MyEnrolmentDto {
    public int EnrolmentId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
    public EventRefDto Event { get; set; } = new();
    public CategoryRefDto Category { get; set; } = new();
}

public class ParticipantRefDto {
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class EventEnrolmentDto {
    public int EnrolmentId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
    public ParticipantRefDto Participant { get; set; } = new();
    public CategoryRefDto Category { get; set; } = new();
}

public class MyResultDto {
    public int ResultId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public DateTime RecordedAt { get; set; }
    public int TotalFinishers { get; set; }
    public EventRefDto Event { get; set; } = new();
    public CategoryRefDto Category { get; set; } = new();
}

public class EventResultDto {
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}

// Requests
public record LoginPayload(string Email, string Password);
public record RegisterPayload(string FirstName, string LastName, string Email, string Password, string Role);
public record ProfilePayload(string FirstName, string LastName, string Email);
public record EventPayload(string Name, string Description, DateTime Date, string Location, decimal Distance, string EventType);
public record CategoryPayload(string Name, string? Description, int? MinimumAge, int? MaximumAge);
public record EnrolPayload(int EventId, int CategoryId);
public record CreateResultPayload(int EnrolmentId, TimeSpan FinishTime, int Position);
public record UpdateResultPayload(TimeSpan FinishTime, int Position);
public record StatusPayload(string Status);
