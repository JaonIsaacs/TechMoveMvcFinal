using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechMove.Models;

namespace TechMove.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // IMPORTANT: Call base first for Identity

            // Configure Client
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
            });

            // Configure Contract
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.SignedAgreementPath).HasMaxLength(500);

                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Contracts)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ServiceRequest
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                
                // Configure currency fields
                entity.Property(e => e.CostUsd).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostZar).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18,6)");
                
                entity.Property(e => e.Status).HasConversion<string>();

                // Ignore computed property
                entity.Ignore(e => e.Cost);

                entity.HasOne(e => e.Contract)
                    .WithMany(c => c.ServiceRequests)
                    .HasForeignKey(e => e.ContractId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed initial data
            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 1, Name = "Acme Corporation", Region = "North America" },
                new Client { Id = 2, Name = "Global Tech Ltd", Region = "Europe" }
            );
        }
    }
}
