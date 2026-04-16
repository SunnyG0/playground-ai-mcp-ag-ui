namespace ChinookApi.Models;
public class Track
{
    public int TrackId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? AlbumId { get; set; }
    public Album? Album { get; set; }
    public int MediaTypeId { get; set; }
    public MediaType? MediaType { get; set; }
    public int? GenreId { get; set; }
    public Genre? Genre { get; set; }
    public string? Composer { get; set; }
    public int Milliseconds { get; set; }
    public int? Bytes { get; set; }
    public decimal UnitPrice { get; set; }
}
