using Microsoft.EntityFrameworkCore;
using LeaveRequests.API.Models;

namespace LeaveRequests.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Type)
             .HasMaxLength(20)
             .IsRequired();

            e.Property(x => x.Status)
             .HasMaxLength(20)
             .IsRequired();

            e.Property(x => x.ReviewerNote)
             .HasMaxLength(500);

            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_LeaveRequests_EndDate", "[EndDate] >= [StartDate]");
                t.HasCheckConstraint("CK_LeaveRequests_Status", "[Status] IN ('Pending', 'Approved', 'Rejected')");
                t.HasCheckConstraint("CK_LeaveRequests_Type", "[Type] IN ('Vacation', 'Sick', 'Unpaid')");
            });

            e.Property(x => x.CreatedAt)
             .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
