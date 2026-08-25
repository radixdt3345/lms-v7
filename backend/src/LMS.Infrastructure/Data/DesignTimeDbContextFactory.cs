using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LMS.Infrastructure.Data;

/// <summary>
/// Allows `dotnet ef` CLI to create a LmsDbContext without a running application host.
/// Connection string is read from the ConnectionStrings__DefaultConnection environment
/// variable (set by CI) or DATABASE_URL, with a local-dev fallback.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LmsDbContext>
{
    public LmsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Port=5432;Database=lms_dev;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<LmsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new LmsDbContext(optionsBuilder.Options);
    }
}
