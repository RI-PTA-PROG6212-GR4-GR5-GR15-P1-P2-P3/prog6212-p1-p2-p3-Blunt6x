using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models.Api;
namespace RaceDay.Web.Models.ViewModels;

public class HomeViewModel {
    public List<EventDto> Events { get; set; } = new();
    public int TotalUpcoming { get; set; }
    public string? Search { get; set; }
    public string? Type { get; set; }
    public decimal? MinDistance { get; set; }
    public decimal? MaxDistance { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Sort { get; set; } = "date";
    public bool IncludePast { get; set; }
    public HashSet<int> EnrolledEventIds { get; set; } = new();
    public bool HasFilters => !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(Type) || MinDistance.HasValue || MaxDistance.HasValue || From.HasValue || To.HasValue || IncludePast;
}

public class EventDetailsViewModel {
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public MyEnrolmentDto? MyEnrolment { get; set; }
    public int SelectedCategoryId { get; set; }
}

public class EventFormViewModel {
    public int? EventId { get; set; }
    [Required(ErrorMessage = "Enter the event name."), StringLength(120)] public string Name { get; set; } = "";
    [StringLength(1000, ErrorMessage = "The description can be at most 1000 characters.")] public string? Description { get; set; }
    [Required(ErrorMessage = "Choose the event date and start time."), DataType(DataType.DateTime)] public DateTime? Date { get; set; }
    [Required(ErrorMessage = "Enter the location."), StringLength(180)] public string Location { get; set; } = "";
    [Required(ErrorMessage = "Enter the distance in kilometres."), Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")] public decimal? Distance { get; set; }
    [Required(ErrorMessage = "Choose an event type."), Display(Name = "Event type")] public string EventType { get; set; } = "Run";
    public string? ExistingBannerUrl { get; set; }
    [Display(Name = "Event banner")] public IFormFile? Banner { get; set; }
    public bool IsEdit => EventId.HasValue;
}

public class CategoryFormViewModel {
    public int EventId { get; set; }
    [Required(ErrorMessage = "Enter a category name."), StringLength(80)] public string Name { get; set; } = "";
    [StringLength(250)] public string? Description { get; set; }
    [Range(0, 120, ErrorMessage = "Enter an age between 0 and 120."), Display(Name = "Minimum age")] public int? MinimumAge { get; set; }
    [Range(0, 120, ErrorMessage = "Enter an age between 0 and 120."), Display(Name = "Maximum age")] public int? MaximumAge { get; set; }
}
