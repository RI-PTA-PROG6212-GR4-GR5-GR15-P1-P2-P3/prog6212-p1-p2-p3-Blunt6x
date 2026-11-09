using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
namespace RaceDay.Api.Data;
public class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options) {
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();
    protected override void OnModelCreating(ModelBuilder b) {
        // EventId is not a convention-based key for a type named RaceEvent.
        // Declare every key explicitly so the model remains unambiguous.
        b.Entity<UserAccount>().HasKey(x => x.UserId);
        b.Entity<RaceEvent>().HasKey(x => x.EventId);
        b.Entity<Category>().HasKey(x => x.CategoryId);
        b.Entity<Enrolment>().HasKey(x => x.EnrolmentId);
        b.Entity<RaceResult>().HasKey(x => x.ResultId);

        // Keep EF's SQL mapping aligned with docs/RaceDayDB.sql.
        b.Entity<UserAccount>().Property(x => x.FirstName).HasMaxLength(50);
        b.Entity<UserAccount>().Property(x => x.LastName).HasMaxLength(50);
        b.Entity<UserAccount>().Property(x => x.Email).HasMaxLength(100);
        b.Entity<UserAccount>().Property(x => x.PasswordHash).HasColumnName("Password").HasMaxLength(255);
        b.Entity<RaceEvent>().Property(x => x.Name).HasColumnName("EventName").HasMaxLength(100);
        b.Entity<RaceEvent>().Property(x => x.Date).HasColumnName("EventDate").HasColumnType("date");
        b.Entity<RaceEvent>().Property(x => x.Location).HasMaxLength(150);
        b.Entity<RaceEvent>().Property(x => x.Distance).HasColumnType("decimal(6,2)");
        b.Entity<Category>().Property(x => x.Name).HasColumnName("CategoryName").HasMaxLength(50);
        b.Entity<Enrolment>().Property(x => x.EnrolmentDate).HasColumnType("date");
        b.Entity<RaceResult>().Property(x => x.FinishTime).HasColumnType("time(0)");
        b.Entity<UserAccount>().HasIndex(x => x.Email).IsUnique();
        b.Entity<UserAccount>().Property(x => x.Role).HasDefaultValue(Roles.Participant);
        b.Entity<RaceEvent>().HasOne(x => x.Organiser).WithMany(x => x.OrganisedEvents).HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<RaceEvent>().ToTable(t => t.HasCheckConstraint("CK_Events_EventType", "[EventType] IN ('Run','Walk','Cycle')"));
        b.Entity<Category>().HasOne(x => x.Event).WithMany(x => x.Categories).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Enrolment>().HasOne(x => x.Participant).WithMany(x => x.Enrolments).HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Enrolment>().HasOne(x => x.Event).WithMany(x => x.Enrolments).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Enrolment>().HasOne(x => x.Category).WithMany(x => x.Enrolments).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Enrolment>().HasIndex(x => new { x.ParticipantId, x.EventId }).IsUnique();
        b.Entity<RaceResult>().HasIndex(x => x.EnrolmentId).IsUnique();
        b.Entity<RaceResult>().HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<RaceResult>(x => x.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
