using PortListener.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register in-memory weight storage service
builder.Services.AddSingleton<IWeightStorageService, WeightStorageService>();

// Register background service for UDP listener
builder.Services.AddHostedService<ScaleListenerService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Add a specific endpoint to return the raw scale data from file (as previous script did)
app.MapGet("/api/data", async (HttpContext context) =>
{
    const string jsonFilePath = "data/scale_data.json";
    if (File.Exists(jsonFilePath))
    {
        string fileContent = await File.ReadAllTextAsync(jsonFilePath);
        var lines = fileContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
            
        // Convert JSONL to JSON Array
        return Results.Content("[" + string.Join(",", lines) + "]", "application/json");
    }
    return Results.Content("[]", "application/json");
});

// Configure to run on all network interfaces on port 7001
app.Urls.Add("http://0.0.0.0:7001");

app.Run();
