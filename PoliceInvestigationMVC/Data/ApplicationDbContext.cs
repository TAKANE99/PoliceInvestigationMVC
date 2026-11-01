using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace PoliceInvestigationMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PoliceStation> PoliceStations { get; set; }
        public DbSet<PoliceOfficer> PoliceOfficers { get; set; }
        public DbSet<Investigation> Investigations { get; set; }
        public DbSet<Evidence> EvidenceRecords { get; set; }
        public DbSet<Witness> Witnesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PoliceStation>()
                .HasMany<PoliceOfficer>()
                .WithOne(o => o.Station)
                .HasForeignKey(o => o.StationId)
                .IsRequired();

            modelBuilder.Entity<PoliceOfficer>()
                .HasMany<Investigation>()
                .WithOne(i => i.InvestigatingOfficer)
                .HasForeignKey(i => i.InvestigatingOfficerId)
                .IsRequired();

            modelBuilder.Entity<Investigation>()
                .HasMany<Evidence>()
                .WithOne(e => e.Investigation)
                .HasForeignKey(e => e.InvestigationId)
                .IsRequired();

            modelBuilder.Entity<Investigation>()
                .HasMany<Witness>()
                .WithOne(w => w.Investigation)
                .HasForeignKey(w => w.InvestigationId)
                .IsRequired();
        }
    }
}