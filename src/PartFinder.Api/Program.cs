using System.Text.Json.Serialization;
using PartFinder.Api.Endpoints;
using PartFinder.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Catalog")
    ?? throw new InvalidOperationException("Connection string 'Catalog' is missing.");
var nhtsaBaseUrl = builder.Configuration["Nhtsa:BaseUrl"] ?? "https://vpic.nhtsa.dot.gov/";

builder.Services.AddInfrastructure(connectionString, nhtsaBaseUrl);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
}

app.MapHealthChecks("/health");
app.MapVinEndpoints();
app.MapVehicleEndpoints();
app.MapCatalogEndpoints();

await app.RunAsync();

public partial class Program;
