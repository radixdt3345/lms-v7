using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Rs256Key> Rs256Keys => Set<Rs256Key>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LeaveType> LeaveTypes { get; set; } = null!;
    public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LmsDbContext).Assembly);
    }
}
