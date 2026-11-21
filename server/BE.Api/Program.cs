using BE.Infrastructure.Database;
using LinqToDB;
using LinqToDB.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Db")
         ?? throw new InvalidOperationException("Connection string 'Db' is missing. Set it with: dotnet user-secrets set \"ConnectionStrings:Db\" \"...\"");

DatabaseMigrator.Migrate(cs);

builder.Services.AddLinqToDBContext<AppDataConnection>((_, options) => options.UsePostgreSQL(cs));
builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(c => c.Title = "Holiday Planner API");

var app = builder.Build();

app.UseOpenApi();
app.UseSwaggerUi();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
