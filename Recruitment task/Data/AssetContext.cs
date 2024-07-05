using Microsoft.EntityFrameworkCore;
using Recruitment_task.Models.Asset;
using Recruitment_task.Models.Bars;

namespace Recruitment_task.Data
{
    public class AssetContext : DbContext
    {
        public DbSet<AssetData> AssetData { get; set; }
        public DbSet<BarsData> BarsDatas { get; set; }


        public AssetContext(DbContextOptions<AssetContext> options) : base(options)
        {
             Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssetData>().HasKey(d => d.id);
            modelBuilder.Entity<BarsData>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<BarsData>()
                .HasOne(b => b.AssetData)
                .WithMany(d => d.barsData)
                .HasForeignKey(b => b.AssetId);
        }
    }
}
