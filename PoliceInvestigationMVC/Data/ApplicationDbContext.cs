using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace PoliceInvestigationMVC.Data
{
    // Defines the database context that connects the application's models and
    // Identity data to the database.
    public class ApplicationDbContext : IdentityDbContext
    {
        // Receives database configuration options from ASP.NET Core through dependency injection.
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
        public DbSet<CaseReview> CaseReviews { get; set; }
        public DbSet<InvestigationOfficer> InvestigationOfficers { get; set; }

        // Configures relationships between the application's database entities
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Applies the default database configuration required by ASP.NET Core Identity.
            base.OnModelCreating(modelBuilder);

            //Ensure every investigation has a unique case number.
            modelBuilder.Entity<Investigation>()
                .HasIndex(i => i.CaseNumber)
                .IsUnique();

            // Ensure every police officer has a unique service number.
            modelBuilder.Entity<PoliceOfficer>()
                .HasIndex(o => o.ServiceNumber)
                .IsUnique();

            // Ensure every evidence record has a unique reference.
            modelBuilder.Entity<Evidence>()
                .HasIndex(e => e.EvidenceReference)
                .IsUnique();

            // ONE-TO-MANY: One station has many officers.
            modelBuilder.Entity<PoliceStation>()
                .HasMany(s => s.PoliceOfficers)
                .WithOne(o => o.Station)
                .HasForeignKey(o => o.StationId)
                .IsRequired();

            // ONE-TO-MANY: One officer can lead many investigations.
            modelBuilder.Entity<PoliceOfficer>()
                .HasMany<Investigation>()
                .WithOne(i => i.InvestigatingOfficer)
                .HasForeignKey(i => i.InvestigatingOfficerId)
                .IsRequired();

            // ONE-TO-MANY: One investigation has many evidence records.
            modelBuilder.Entity<Investigation>()
                .HasMany<Evidence>()
                .WithOne(e => e.Investigation)
                .HasForeignKey(e => e.InvestigationId)
                .IsRequired();

            // ONE-TO-MANY: One police officer can collect many evidence records.
            modelBuilder.Entity<Evidence>()
                .HasOne(e => e.CollectedBy)
                .WithMany()
                .HasForeignKey(e => e.CollectedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // ONE-TO-MANY: One investigation has many witnesses.
            modelBuilder.Entity<Investigation>()
                .HasMany<Witness>()
                .WithOne(w => w.Investigation)
                .HasForeignKey(w => w.InvestigationId)
                .IsRequired();

            // Configure a one-to-one relationship between Investigation and CaseReview.
            modelBuilder.Entity<Investigation>()
                .HasOne(i => i.CaseReview)
                .WithOne(r => r.Investigation)
                .HasForeignKey<CaseReview>(r => r.InvestigationId);

            // MANY-TO-MANY: Configure the joining table's composite key.
            modelBuilder.Entity<InvestigationOfficer>()
                .HasKey(io => new 
                { 
                    io.InvestigationId, 
                    io.PoliceOfficerId 
                });

            // Connect the joining table to Investigation.
            modelBuilder.Entity<InvestigationOfficer>()
                .HasOne(io => io.Investigation)
                .WithMany(i => i.InvestigationOfficers)
                .HasForeignKey(io => io.InvestigationId);

            // Connect the joining table to PoliceOfficer.
            modelBuilder.Entity<InvestigationOfficer>()
                .HasOne(io => io.PoliceOfficer)
                .WithMany(o => o.InvestigationOfficers)
                .HasForeignKey(io => io.PoliceOfficerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}