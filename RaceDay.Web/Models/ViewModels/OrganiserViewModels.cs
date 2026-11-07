using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models.Api;
namespace RaceDay.Web.Models.ViewModels;

public class DashboardViewModel {
    public string OrganiserName { get; set; } = "";
    public List<EventDto> Events { get; set; } = new();
    public int TotalEvents => Events.Count;
    public int UpcomingEvents => Events.Count(e => e.IsUpcoming);
    public int TotalEnrolments => Events.Sum(e => e.EnrolmentCount);
    public int ResultsPublished => Events.Sum(e => e.ResultCount);
    public EventDto? NextEvent => Events.Where(e => e.IsUpcoming).OrderBy(e => e.Date).FirstOrDefault();
}

/// <summary>Base for organiser pages that work on one event (categories, enrolments, results) and let the organiser switch events.</summary>
public class EventScopedViewModel {
    public List<EventDto> MyEvents { get; set; } = new();
    public EventDto? Selected { get; set; }
}

public class CategoriesPageViewModel : EventScopedViewModel {
    public List<CategoryDto> Categories { get; set; } = new();
    public CategoryFormViewModel Form { get; set; } = new();
}

public class EnrolmentsPageViewModel : EventScopedViewModel {
    public List<EventEnrolmentDto> Enrolments { get; set; } = new();
    public string? StatusFilter { get; set; }
}

public class ResultRowViewModel {
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string Status { get; set; } = "";
    public int? ResultId { get; set; }
    public string? FinishTime { get; set; }
    public int? Position { get; set; }
}

public class ResultsPageViewModel : EventScopedViewModel {
    public List<ResultRowViewModel> Rows { get; set; } = new();
    public bool EventConcluded { get; set; }
}

public class SaveResultViewModel {
    public int EventId { get; set; }
    [Required] public int EnrolmentId { get; set; }
    public int? ResultId { get; set; }
    [Required(ErrorMessage = "Enter the finish time as hh:mm:ss.")] public string? FinishTime { get; set; }
    [Required(ErrorMessage = "Enter the finishing position."), Range(1, 100000, ErrorMessage = "Position must be 1 or higher.")] public int? Position { get; set; }
}
