using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Mcp;
using System.Text.Json.Serialization;

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

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapMcp("/mcp");

app.Run();
