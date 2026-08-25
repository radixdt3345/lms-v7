using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. "
            + "Set ConnectionStrings__DefaultConnection environment variable."
    );

builder.Services.AddDbContext<LmsDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
