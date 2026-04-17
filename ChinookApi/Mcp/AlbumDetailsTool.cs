using System.ComponentModel;
using System.Text;
using MediatR;
using ModelContextProtocol.Server;
using ChinookApi.Features.Albums;
using ChinookApi.Features.Tracks;

namespace ChinookApi.Mcp;

[McpServerToolType]
public sealed class AlbumDetailsTool(IMediator mediator)
{
    [Description("Get complete details for an album — title, artist, and the full track listing with track names, durations, genres, and composers. Use this when a user wants to know what songs are on a specific album.")]
    [McpServerTool(Name = "get_album_details")]
    public async Task<string> GetAlbumDetailsAsync(
        [Description("The album title (or part of it) to look up")] string albumTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(albumTitle))
            return "Please provide an album title.";

        var albums = await mediator.Send(new GetAllAlbumsQuery(albumTitle, 1, 5), cancellationToken);

        if (albums.Items.Count == 0)
            return $"No album found matching \"{albumTitle}\".";

        var sb = new StringBuilder();

        if (albums.Items.Count > 1)
        {
            sb.AppendLine($"Multiple albums matched \"{albumTitle}\". Showing all:");
            sb.AppendLine();
        }

        foreach (var album in albums.Items)
        {
            var tracks = await mediator.Send(new GetTracksByAlbumQuery(album.AlbumId), cancellationToken);
            var totalMs = tracks.Sum(t => t.Milliseconds);
            var totalDuration = TimeSpan.FromMilliseconds(totalMs);

            sb.AppendLine($"Album:  {album.Title} (ID: {album.AlbumId})");
            sb.AppendLine($"Artist: {album.Artist?.Name}");
            sb.AppendLine($"Tracks: {tracks.Count}  |  Total duration: {(int)totalDuration.TotalMinutes}:{totalDuration.Seconds:D2}");
            sb.AppendLine();
            sb.AppendLine("Track listing:");

            for (int i = 0; i < tracks.Count; i++)
            {
                var t = tracks[i];
                var duration = TimeSpan.FromMilliseconds(t.Milliseconds);
                var composer = string.IsNullOrWhiteSpace(t.Composer) ? "" : $"  composer: {t.Composer}";
                sb.AppendLine($"  {i + 1,2}. [{t.TrackId}] {t.Name}  ({duration:m\\:ss})  [{t.Genre?.Name}]{composer}");
            }

            if (albums.Items.Count > 1)
                sb.AppendLine(new string('-', 40));
        }

        return sb.ToString();
    }
}
