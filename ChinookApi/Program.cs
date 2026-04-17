using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Mcp;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddDbContext<ChinookContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Chinook") ?? "Data Source=Chinook.db"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<SearchMusicTool>()
    .WithTools<ArtistCatalogTool>()
    .WithTools<AlbumDetailsTool>()
    .WithTools<PlaylistTool>()
    .WithTools<CustomerHistoryTool>()
    .WithTools<GenreExplorerTool>();

var openAiApiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? string.Empty;
var openAiModel = builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini";

builder.Services.AddSingleton<IChatClient>(_ =>
    new OpenAIClient(openAiApiKey).GetChatClient(openAiModel).AsIChatClient());

builder.Services.AddHttpClient("mcp");

var app = builder.Build();

if (string.IsNullOrEmpty(openAiApiKey))
    app.Logger.LogWarning("OpenAI API key is not configured. Set 'OpenAI:ApiKey' in configuration or the 'OPENAI_API_KEY' environment variable for the /agent endpoint to work.");

app.UseCors();
app.MapControllers();
app.MapMcp("/mcp");

app.Run();
