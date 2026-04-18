using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Mcp;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

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

builder.Services.AddSingleton<AIAgent>(_ =>
{
    ChatClient chatClient = new OpenAIClient(openAiApiKey).GetChatClient(openAiModel);
    return chatClient.AsAIAgent(
        name: "ChinookAssistant",
        instructions:
            "You are a helpful music catalog assistant for the Chinook database. " +
            "You can search for artists, albums, tracks, playlists, genres, and customer purchase history. " +
            "Use the available tools to look up information and provide clear, concise answers.");
});

builder.Services.AddHttpClient("mcp");

var app = builder.Build();

if (string.IsNullOrEmpty(openAiApiKey))
    app.Logger.LogWarning("OpenAI API key is not configured. Set 'OpenAI:ApiKey' in configuration or the 'OPENAI_API_KEY' environment variable for the /agent endpoint to work.");

app.UseCors();
app.MapControllers();
app.MapMcp("/mcp");

app.Run();
