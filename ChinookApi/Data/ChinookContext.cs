using Microsoft.EntityFrameworkCore;
using ChinookApi.Models;

namespace ChinookApi.Data;

public class ChinookContext : DbContext
{
    public ChinookContext(DbContextOptions<ChinookContext> options) : base(options) { }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Chinook DB uses singular table names
        modelBuilder.Entity<Artist>().ToTable("Artist");
        modelBuilder.Entity<Album>().ToTable("Album");
        modelBuilder.Entity<Genre>().ToTable("Genre");
        modelBuilder.Entity<MediaType>().ToTable("MediaType");
        modelBuilder.Entity<Track>().ToTable("Track");
        modelBuilder.Entity<Playlist>().ToTable("Playlist");
        modelBuilder.Entity<Customer>().ToTable("Customer");
        modelBuilder.Entity<Invoice>().ToTable("Invoice");
        modelBuilder.Entity<InvoiceLine>().ToTable("InvoiceLine");

        modelBuilder.Entity<Playlist>()
            .HasMany(p => p.Tracks)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "PlaylistTrack",
                j => j.HasOne<Track>().WithMany().HasForeignKey("TrackId"),
                j => j.HasOne<Playlist>().WithMany().HasForeignKey("PlaylistId")
            );

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Lines)
            .WithOne()
            .HasForeignKey(l => l.InvoiceId);
    }
}
