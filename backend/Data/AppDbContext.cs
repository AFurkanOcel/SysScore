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
                entity.Property(data => data.SwapUsage).IsRequired();
                entity.Property(data => data.DiskFreeGb).IsRequired();
                entity.Property(data => data.ProcessCount).IsRequired();
                entity.Property(data => data.HighCpuProcessCount).IsRequired();
                entity.Property(data => data.HighMemoryProcessCount).IsRequired();
                entity.Property(data => data.NetworkConnectionCount).IsRequired();
                entity.Property(data => data.ListeningPortCount).IsRequired();
                entity.Property(data => data.EstablishedConnectionCount).IsRequired();
                entity.Property(data => data.SynSentConnectionCount).IsRequired();
                entity.Property(data => data.TimeWaitConnectionCount).IsRequired();
                entity.Property(data => data.UniqueRemoteAddressCount).IsRequired();
                entity.Property(data => data.UniqueRemotePortCount).IsRequired();
                entity.Property(data => data.NetworkConnectionDelta).IsRequired();
                entity.Property(data => data.OutboundPacketRate).IsRequired();
                entity.Property(data => data.InboundPacketRate).IsRequired();
                entity.Property(data => data.SystemUptimeSeconds).IsRequired();
                entity.Property(data => data.BootTime).IsRequired();
                entity.Property(data => data.UnnecessaryFileCount).IsRequired();
                entity.Property(data => data.UnnecessaryFileSizeMb).IsRequired();
                entity.Property(data => data.UnnecessaryFileLocations).HasMaxLength(500);
                entity.Property(data => data.LargestUnnecessaryFiles).HasMaxLength(8000);
                entity.Property(data => data.Timestamp).IsRequired();
                entity.Property(data => data.SecurityScore).IsRequired();
                entity.Property(data => data.Explanation).HasMaxLength(1000);
                entity.Property(data => data.ThreatType).HasMaxLength(120);
                entity.Property(data => data.ThreatLevel).HasMaxLength(30);
                entity.Property(data => data.ThreatEvidence).HasMaxLength(1500);
                entity.Property(data => data.RecommendedActions).HasMaxLength(1500);
            });
        }
    }
}
