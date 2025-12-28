using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class FilesDbContext : DbContext
{
    public FilesDbContext(DbContextOptions<FilesDbContext> options)
        : base(options)
    {
    }

    public FilesDbContext()
    { }

    public DbSet<FileEntity> Files { get; set; }
    public DbSet<FileDetails> FileDetails { get; set; }
    public DbSet<DownloadAnalytics> DownloadAnalytics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=database.db");
        }
    }
}