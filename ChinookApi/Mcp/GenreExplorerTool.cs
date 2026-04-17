using System.ComponentModel;
using System.Text;
using MediatR;
using ModelContextProtocol.Server;
using ChinookApi.Features.Genres;
using ChinookApi.Features.Tracks;

namespace ChinookApi.Mcp;

[McpServerToolType]
public sealed class GenreExplorerTool(IMediator mediator)
{
    [Description("List all available music genres in the catalog. Use this to discover what genres exist before calling discover_by_genre.")]
    [McpServerTool(Name = "list_genres")]
    public async Task<string> ListGenresAsync(CancellationToken cancellationToken = default)
    {
        var genres = await mediator.Send(new GetAllGenresQuery(), cancellationToken);

        if (genres.Count == 0)
            return "No genres found.";

        var sb = new StringBuilder();
        sb.AppendLine($"Available genres ({genres.Count} total):");
        foreach (var g in genres)
            sb.AppendLine($"  • [{g.GenreId}] {g.Name}");

        return sb.ToString();
    }

    [Description("Browse tracks available in a specific music genre (e.g. Rock, Jazz, Classical). Returns a paginated list of tracks with their album and duration. Use list_genres to see available genre names.")]
    [McpServerTool(Name = "discover_by_genre")]
    public async Task<string> DiscoverByGenreAsync(
        [Description("The genre name to browse (e.g. 'Rock', 'Jazz', 'Classical')")] string genreName,
        [Description("Page number for pagination (default: 1)")] int page = 1,
        [Description("Number of tracks to return per page (default: 20, max: 50)")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(genreName))
            return "Please provide a genre name.";

        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);

        var genres = await mediator.Send(new GetAllGenresQuery(), cancellationToken);
        var genre = genres.FirstOrDefault(g =>
            g.Name != null && g.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase));

        if (genre is null)
        {
            var partial = genres.Where(g => g.Name != null && g.Name.Contains(genreName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (partial.Count == 0)
                return $"No genre found matching \"{genreName}\". Use list_genres to see available genres.";

            if (partial.Count == 1)
                genre = partial[0];
            else
            {
                var sb2 = new StringBuilder();
                sb2.AppendLine($"Multiple genres matched \"{genreName}\". Please be more specific:");
                foreach (var g in partial)
                    sb2.AppendLine($"  • [{g.GenreId}] {g.Name}");
                return sb2.ToString();
            }
        }

        var tracks = await mediator.Send(new GetAllTracksQuery(null, genre.GenreId, page, pageSize), cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine($"Genre: {genre.Name}  —  {tracks.Total} tracks total");
        sb.AppendLine($"Page {tracks.Page} of {(int)Math.Ceiling(tracks.Total / (double)tracks.PageSize)}  (showing {tracks.Items.Count})");
        sb.AppendLine();

        foreach (var t in tracks.Items)
        {
            var duration = TimeSpan.FromMilliseconds(t.Milliseconds);
            sb.AppendLine($"  • [{t.TrackId}] {t.Name}  —  {t.Album?.Title}  ({duration:m\\:ss})");
        }

        return sb.ToString();
    }
}
