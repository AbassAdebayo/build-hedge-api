using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Context
{
    public class BuildHedgeContext(DbContextOptions<BuildHedgeContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            SeedRoleData(modelBuilder);
            SeedDomainRules(modelBuilder);

            // 1. Define the Composite Key (User + Org must be unique)
            modelBuilder.Entity<UserOrganizationMembership>(entity =>
            {
                // Composite Key
                entity.HasKey(m => new { m.UserId, m.OrganizationId });

                // Link to User
                entity.HasOne(m => m.User)
                    .WithMany(u => u.Memberships)
                    .HasForeignKey(m => m.UserId);

                // Link to Organization
                entity.HasOne(m => m.Organization)
                    .WithMany(o => o.Memberships)
                    .HasForeignKey(m => m.OrganizationId);
            });

            modelBuilder.Entity<HedgeContract>(entity =>
            {
                entity.Property(e => e.LockedPrice).HasPrecision(18, 4);
                entity.Property(e => e.PremiumFee).HasPrecision(18, 4);
                entity.Property(e => e.Quantity).HasPrecision(18, 4);
            });

            modelBuilder.Entity<MaterialPriceHistory>()
           .Property(m => m.Price)
           .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(p => p.TotalBudget).HasPrecision(18, 2);
                entity.HasOne(p => p.Organization)
                .WithMany(o => o.Projects)
                .HasForeignKey(p => p.OrganizationId);

            });

            // Map Material Metadata to JSONB for AI flexibility
            modelBuilder.Entity<Material>()
                .Property(m => m.MetadataJson)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Organization>()
                .HasIndex(org => org.BusinessName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(string));

                foreach (var property in properties)
                {
                    property.SetValueConverter(new ValueConverter<string, string>(
                        v => v.Trim().ToLower(),
                        v => v                   
                    ));
                }
            }
        }

        private void SeedRoleData(ModelBuilder modelBuilder)
        {
            var createdBy = "HedgeSystem";
            var roles = new List<Role>
            {
                new Role
                {
                    Id = new Guid("a45c9e02-1f0b-4e57-b3d8-9b77b4a302be"),
                    Name = "Hedge_Admin",
                    Description = "Corporate executive with full financial approval authority.",
                    CreatedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 02, 03), DateTimeKind.Utc),
                    CreatedBy = createdBy
                },
                new Role
                {
                    Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6209d4a871"),
                    Name = "Hedge_Editor",
                    Description = "Project manager/Contractor who can request price locks.",
                    CreatedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 02, 03), DateTimeKind.Utc),
                    CreatedBy = createdBy
                },
                new Role
                {
                    Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6498d4a871"),
                    Name = "Hedge_Viewer",
                    Description = "Stakeholder who can only view risk reports.",
                    CreatedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 02, 03), DateTimeKind.Utc),
                    CreatedBy = createdBy
                }
            };

            modelBuilder.Entity<Role>().HasData(roles);
        }

        private  void SeedDomainRules(ModelBuilder modelBuilder)
        {
            var creator = "HedgeSystem";
            var timeCreated = DateTime.SpecifyKind(new DateTime(2026, 02, 09), DateTimeKind.Utc);
            modelBuilder.Entity<DomainRule>().HasData(
                new DomainRule { Id = new Guid("6e3e8978-dcb0-42ea-9c78-7f6209d4a871"), DomainName = "gmail.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = timeCreated },
                new DomainRule { Id = new Guid("9f3d4978-dcb0-42ea-9c48-7f8509d4a871"), DomainName = "yahoo.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = timeCreated },
                new DomainRule { Id = new Guid("6e3d4962-dcb0-42bc-9c58-7f6209d4a871"), DomainName = "hotmail.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = timeCreated },
                new DomainRule {Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6521d4a871"), DomainName = "outlook.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = timeCreated },
                new DomainRule { Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6209e5b871"), DomainName = "greatmoh007@gmail.com", IsAllowed = true, Note = "Developer testing bypass", CreatedBy = creator, CreatedAtUtc = timeCreated }

            );
            
        }

        DbSet<HedgeContract> HedgeContracts => Set<HedgeContract>();
        DbSet<Material> Materials => Set<Material>();
        DbSet<MaterialPriceHistory> MaterialPriceHistories => Set<MaterialPriceHistory>();
        DbSet<Organization> Organizations => Set<Organization>();
        DbSet<Project> Projects => Set<Project>();
        DbSet<Role> Roles => Set<Role>();
        DbSet<User> Users => Set<User>();
        DbSet<UserRole> UserRoles => Set<UserRole>();
        DbSet<DomainRule> DomainRules => Set<DomainRule>();
    }
}
