using System.Security.Cryptography;
using LMS.Infrastructure.AuditLogs;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Departments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. "
            + "Set ConnectionStrings__DefaultConnection environment variable."
    );

builder.Services.AddDbContext<LmsDbContext>(options => options.UseNpgsql(connectionString));

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Named HttpClient for AuthService (Azure AD token exchanges)
builder.Services.AddHttpClient<IAuthService, AuthService>();

// ── JWT Bearer authentication ─────────────────────────────────────────────────
// The RSA public key is loaded dynamically from the active Rs256Key in the database.
// We configure a custom token validation parameters factory so the key can rotate
// without restarting the application.
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt__Issuer"] ?? "lms-api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt__Audience"] ?? "lms-client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidateIssuerSigningKey = true,
            // The signing key is resolved at token-validation time so key rotation
            // is reflected immediately without restart.
            IssuerSigningKeyResolver = (_, _, kid, _) =>
            {
                // Build a temporary scope to resolve the db and fetch the active key.
                // This runs synchronously inside the JWT middleware; DbContext is fast here.
                var sp = builder.Services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
                var keys = db.Rs256Keys.Where(k => k.IsActive).ToList();
                return keys
                    .Where(k => kid == null || k.Id.ToString() == kid)
                    .Select(k =>
                    {
                        var rsa = RSA.Create();
                        rsa.ImportRSAPublicKey(Convert.FromBase64String(k.PublicKey), out _);
                        return (SecurityKey)
                            new RsaSecurityKey(rsa) { KeyId = k.Id.ToString() };
                    });
            },
            RoleClaimType = "role",
            NameClaimType = "name",
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                var problem = System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        title = "Unauthorized",
                        detail = "A valid bearer token is required.",
                        status = 401,
                    }
                );
                return context.Response.WriteAsync(problem);
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";
                var problem = System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        title = "Forbidden",
                        detail = "You do not have permission to perform this action.",
                        status = 403,
                    }
                );
                return context.Response.WriteAsync(problem);
            },
        };
    });

builder.Services.AddAuthorization();

// ── Controllers + ProblemDetails ─────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ── Startup tasks ─────────────────────────────────────────────────────────────
// Ensure an active RS256 signing key exists before accepting traffic.
using (var scope = app.Services.CreateScope())
{
    var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
    await jwt.EnsureActiveKeyAsync();
}

app.Run();
