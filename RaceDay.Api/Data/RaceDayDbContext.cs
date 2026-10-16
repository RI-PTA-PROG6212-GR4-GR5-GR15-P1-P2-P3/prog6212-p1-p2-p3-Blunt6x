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
        b.Entity<RaceResult>().Property(x => x.FinishTime).HasConversion(v => v.ToString(), v => TimeSpan.Parse(v));
    }
}
