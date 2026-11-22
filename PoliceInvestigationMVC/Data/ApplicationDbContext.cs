using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace PoliceInvestigationMVC.Data
{
    // Defines the database context that connects the application's models and
    // Identity data to the database.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Receives database configuration options from ASP.NET Core through dependency injection.
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // Defines the database tables used to store the applicatio's data.
        public DbSet<PoliceStation> PoliceStations { get; set; }
        public DbSet<PoliceOfficer> PoliceOfficers { get; set; }
        public DbSet<Investigation> Investigations { get; set; }
        public DbSet<InvestigationSuspect> InvestigationSuspects { get; set; }
        // Stores progress updates recorded during investigations.
        public DbSet<InvestigationProgress> InvestigationProgressRecords { get; set; }
        public DbSet<Evidence> EvidenceRecords { get; set; }
        public DbSet<EvidenceFile> EvidenceFiles { get; set; }
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

            // ONE-TO-ONE: Link a police officer to their login account.
            // Each officer can have one ApplicationUser account.
            // An officer record can exist before a login account is linked.
            modelBuilder.Entity<PoliceOfficer>()
                .HasOne(o => o.UserAccount)
                .WithOne()
                .HasForeignKey<PoliceOfficer>(o => o.ApplicationUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

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
            // An investigation can exist without an assigned officer.
            modelBuilder.Entity<PoliceOfficer>()
                .HasMany<Investigation>()
                .WithOne(i => i.InvestigatingOfficer)
                .HasForeignKey(i => i.InvestigatingOfficerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

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

            // Configures a one-to-many relationship between Evidence and EvidenceFile.
            modelBuilder.Entity<Evidence>()
                .HasMany(e => e.EvidenceFiles)
                .WithOne(ef => ef.Evidence)
                .HasForeignKey(ef => ef.EvidenceId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Configures a one-to-many relationship between Investigation and InvestigationSuspect.
            modelBuilder.Entity<Investigation>()
                .HasMany(i => i.InvestigationSuspects)
                .WithOne(s => s.Investigation)
                .HasForeignKey(s => s.InvestigationId)
                .OnDelete(DeleteBehavior.Cascade);

            // An investigation can have many progress updates.
            modelBuilder.Entity<InvestigationProgress>()
                .HasOne(p => p.Investigation)
                .WithMany(i => i.ProgressUpdates)
                .HasForeignKey(p => p.InvestigationId)
                .OnDelete(DeleteBehavior.Cascade);

            // A police officer can record many progress updates.
            modelBuilder.Entity<InvestigationProgress>()
                .HasOne(p => p.PoliceOfficer)
                .WithMany(o => o.ProgressUpdates)
                .HasForeignKey(p => p.PoliceOfficerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}