using Credite.Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// A connection string vem de User Secrets (desenvolvimento) ou variáveis de ambiente.
// Nunca fica em appsettings.json nem no repositório.
var connectionString = builder.Configuration.GetConnectionString("CrediteDb")
    ?? throw new InvalidOperationException(
        "Connection string 'CrediteDb' não configurada. Em desenvolvimento, use User Secrets.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Credite.Identity.Api"
}));

app.Run();

public partial class Program { }
