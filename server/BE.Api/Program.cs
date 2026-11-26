using System.Text.Json.Serialization;
using BE.Api.Auth;
using BE.Infrastructure.Database;
using LinqToDB;
using LinqToDB.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag;
using NSwag.Generation.Processors.Security;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Db")
         ?? throw new InvalidOperationException("Connection string 'Db' is missing. Set it with: dotnet user-secrets set \"ConnectionStrings:Db\" \"...\"");

DatabaseMigrator.Migrate(cs);

builder.Services.AddLinqToDBContext<AppDataConnection>((_, options) => options.UsePostgreSQL(cs));
builder.Services.AddControllers()
    // Enums as text ("Accountant") instead of numbers, in JSON and in the generated TypeScript client.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// --- Authentication (who are you?) -------------------------------------------------------------
// The signing key is a secret: user-secrets locally, environment variable Jwt__Key in the cloud.
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key is missing or shorter than 32 characters. Set it with: dotnet user-secrets set \"Jwt:Key\" \"<random 64 chars>\"");

builder.Services.AddOptions<JwtOptions>().Bind(builder.Configuration.GetSection(JwtOptions.Section)).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep "sub", "role", ... as they are in the token
        options.TokenValidationParameters = TokenService.CreateValidationParameters(jwt);
    });

// --- Authorization (what may you do?) – see Auth/Policies.cs and README.md ----------------------
builder.Services.AddAuthorization(options => options.AddHolidayPlannerPolicies());
builder.Services.AddRateLimiter(options => options.AddHolidayPlannerRateLimits());

builder.Services.AddOpenApiDocument(c =>
{
    c.Title = "Holiday Planner API";
    // Adds the "Authorize" button to Swagger UI: paste the token from POST /api/auth/login.
    c.AddSecurity("Bearer", new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token from POST /api/auth/login",
    });
    c.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

var app = builder.Build();

await SystemOwnerBootstrapper.EnsureSystemOwnerAsync(app.Services, app.Configuration, app.Logger);

app.UseOpenApi();
app.UseSwaggerUi();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Lets integration tests (WebApplicationFactory<Program>) reference the app.
public partial class Program;
