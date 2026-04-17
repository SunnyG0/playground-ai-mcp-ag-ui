using System.ComponentModel;
using System.Text;
using MediatR;
using ModelContextProtocol.Server;
using ChinookApi.Features.Albums;
using ChinookApi.Features.Artists;
using ChinookApi.Features.Tracks;

namespace ChinookApi.Mcp;

[McpServerToolType]
public sealed class SearchMusicTool(IMediator mediator)
{
    [Description("Search across the entire music catalog — artists, albums, and tracks — in a single query. Returns the top matches from each category so you can quickly find what you're looking for.")]
    [McpServerTool(Name = "search_music")]
    public async Task<string> SearchMusicAsync(
        [Description("The search term to look for (e.g. artist name, album title, or song name)")] string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Please provide a search term.";

        var artistsTask = mediator.Send(new GetAllArtistsQuery(query, 1, 5), cancellationToken);
        var albumsTask = mediator.Send(new GetAllAlbumsQuery(query, 1, 5), cancellationToken);
        var tracksTask = mediator.Send(new GetAllTracksQuery(query, null, 1, 10), cancellationToken);

        await Task.WhenAll(artistsTask, albumsTask, tracksTask);

        var artists = artistsTask.Result;
        var albums = albumsTask.Result;
        var tracks = tracksTask.Result;

        var sb = new StringBuilder();
        sb.AppendLine($"Search results for \"{query}\":");

        sb.AppendLine($"\nArtists ({artists.Total} total, showing {artists.Items.Count}):");
        if (artists.Items.Count == 0)
            sb.AppendLine("  (none found)");
        else
            foreach (var a in artists.Items)
                sb.AppendLine($"  • [{a.ArtistId}] {a.Name}");

        sb.AppendLine($"\nAlbums ({albums.Total} total, showing {albums.Items.Count}):");
        if (albums.Items.Count == 0)
            sb.AppendLine("  (none found)");
        else
            foreach (var al in albums.Items)
                sb.AppendLine($"  • [{al.AlbumId}] {al.Title}  —  {al.Artist?.Name}");

        sb.AppendLine($"\nTracks ({tracks.Total} total, showing {tracks.Items.Count}):");
        if (tracks.Items.Count == 0)
            sb.AppendLine("  (none found)");
        else
            foreach (var t in tracks.Items)
            {
                var duration = TimeSpan.FromMilliseconds(t.Milliseconds);
                sb.AppendLine($"  • [{t.TrackId}] {t.Name}  —  {t.Album?.Title}  ({duration:m\\:ss})  [{t.Genre?.Name}]");
            }

        return sb.ToString();
    }
}
