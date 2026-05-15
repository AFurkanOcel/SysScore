using Microsoft.EntityFrameworkCore;
using SysScore.Models;

namespace SysScore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<SystemData> SystemDataRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemData>(entity =>
            {
                entity.ToTable("SystemDataRecords");
                entity.HasKey(data => data.Id);

                entity.Property(data => data.CpuUsage).IsRequired();
                entity.Property(data => data.RamUsage).IsRequired();
                entity.Property(data => data.DiskUsage).IsRequired();
                entity.Property(data => data.ProcessCount).IsRequired();
                entity.Property(data => data.Timestamp).IsRequired();
                entity.Property(data => data.SecurityScore).IsRequired();
                entity.Property(data => data.Explanation).HasMaxLength(1000);
            });
        }
    }
}
