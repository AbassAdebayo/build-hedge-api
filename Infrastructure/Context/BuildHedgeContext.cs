using Application.Tenant;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infrastructure.Context
{
    public class BuildHedgeContext(DbContextOptions<BuildHedgeContext> options,
        ITenantProvider tenantProvider) : DbContext(options)
    {
        private readonly Guid _tenantId = tenantProvider.GetTenantId();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            SeedRoleData(modelBuilder);
            SeedDomainRules(modelBuilder);
            SeedCurrencies(modelBuilder);
            SeedHedgeOwnerData(modelBuilder);

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
                entity.Property(e => e.ExchangeRateAtLock).HasPrecision(18, 4);
                entity.Property(e => e.TotalValueBaseCurrency).HasPrecision(18, 4);
            });

            modelBuilder.Entity<MaterialPriceHistory>()
           .Property(m => m.Price)
           .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(p => p.TotalBudget).HasPrecision(18, 2);
                entity.HasOne(p => p.Organization)
                .WithMany(o => o.Projects)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Project>()
                .HasQueryFilter(p => p.OrganizationId == _tenantId);

            modelBuilder.Entity<HedgeContract>()
                .HasQueryFilter(h => h.OrganizationId == _tenantId);

            modelBuilder.Entity<Material>()
                .HasQueryFilter(m => m.OrganizationId == _tenantId);

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

           
        }

        private static void SeedHedgeOwnerData(ModelBuilder modelBuilder)
        {

            var hedgeOwnerRoleId = new Guid("d2719e67-52f4-4f9c-bdb2-123456789abc");
            var hedgeOwnerUserId = new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42");
            var hedgeOwnerOrganizationId = new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42");
            string hedgeOwnerFirstName = "Hedge";
            string hedgeOwnerLastName = "Controller";
            string creator = "Hedge_System";

            var hedgeOwnerRole = new Role
            {
                Id = hedgeOwnerRoleId,
                Name = "Hedge_Owner",
                Description = "Sees total revenue, manages global fees, and views all Orgs, Edit App settings.",
                CreatedAtUtc = SeedDate,
                CreatedBy = creator
            };


            var hedgeOwnerUser = new User
            {
                Id = hedgeOwnerUserId,
                FirstName = hedgeOwnerFirstName,
                LastName = hedgeOwnerLastName,
                Email = "controller@hedge.com",
                PasswordHash = "AQAAAAEAACcQAAAAEP2pYoh7N/0gJ7DyDXZp2oc62m9yeip7DrFBKr5u43ZlnJVvciJFghhjmow0DkG2Zg==",
                PhoneNumber = "+2349117690426",
                IsVerified = true,
                CreatedAtUtc = SeedDate,
                CreatedBy = creator,

            };

            var hedgeOwnerOrganization = new Organization
            {
                Id = hedgeOwnerOrganizationId,
                BusinessName = "Build Hedge",
                BaseCurrencyCode = "NGN",
                IsActive = true,
                SubscriptionPlan = Domain.Contracts.Enum.SubscriptionPlan.Enterprise,
                CreatedAtUtc = SeedDate,
                CreatedBy = creator
            };

            var hedgeOwnerMembership = new UserOrganizationMembership
            {
                Id = new Guid("7ad9b1e1-4c23-46a2-b8e4-219ab417f71f"),
                UserId = hedgeOwnerUserId,
                OrganizationId = hedgeOwnerOrganizationId,
                RoleInOrganization = hedgeOwnerRole.Name,
                JoinedAtUtc = SeedDate,
                CreatedBy = creator
            };


            modelBuilder.Entity<Role>().HasData(hedgeOwnerRole);
            modelBuilder.Entity<User>().HasData(hedgeOwnerUser);
            modelBuilder.Entity<Organization>().HasData(hedgeOwnerOrganization);
            modelBuilder.Entity<UserOrganizationMembership>().HasData(hedgeOwnerMembership);
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
                    CreatedAtUtc = SeedDate,
                    CreatedBy = createdBy
                },
                new Role
                {
                    Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6209d4a871"),
                    Name = "Hedge_Editor",
                    Description = "Project manager/Contractor who can request price locks.",
                    CreatedAtUtc = SeedDate,
                    CreatedBy = createdBy
                },
                new Role
                {
                    Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6498d4a871"),
                    Name = "Hedge_Viewer",
                    Description = "Stakeholder who can only view risk reports.",
                    CreatedAtUtc = SeedDate,
                    CreatedBy = createdBy
                }
            };

            modelBuilder.Entity<Role>().HasData(roles);
        }

        private void SeedDomainRules(ModelBuilder modelBuilder)
        {
            var creator = "HedgeSystem";
            modelBuilder.Entity<DomainRule>().HasData(
                new DomainRule { Id = new Guid("6e3e8978-dcb0-42ea-9c78-7f6209d4a871"), DomainName = "gmail.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = SeedDate },
                new DomainRule { Id = new Guid("9f3d4978-dcb0-42ea-9c48-7f8509d4a871"), DomainName = "yahoo.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = SeedDate },
                new DomainRule { Id = new Guid("6e3d4962-dcb0-42bc-9c58-7f6209d4a871"), DomainName = "hotmail.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = SeedDate },
                new DomainRule {Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6521d4a871"), DomainName = "outlook.com", IsAllowed = false, Note = "Public Provider - Blocked for Org Setup", CreatedBy = creator, CreatedAtUtc = SeedDate },
                new DomainRule { Id = new Guid("6e3d4978-dcb0-42ea-9c48-7f6209e5b871"), DomainName = "greatmoh007@gmail.com", IsAllowed = true, Note = "Developer testing bypass", CreatedBy = creator, CreatedAtUtc = SeedDate }

            );
            
        }

        private void SeedCurrencies(ModelBuilder modelBuilder)
        {
            var createdBy = "HedgeSystem";
            modelBuilder.Entity<Currency>().HasData(
                new Currency
                {
                    Id = new Guid("d3b07384-d9a4-4352-8d0b-6060c57c4c41"),
                    Code = "USD",
                    Name = "US Dollar",
                    Symbol = "$",
                    CreatedAtUtc = SeedDate,
                    CreatedBy = createdBy,
                },
                new Currency
                {
                    Id = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                    Code = "NGN",
                    Name = "Nigerian Naira",
                    Symbol = "₦",
                    CreatedAtUtc = SeedDate,
                    CreatedBy = createdBy,
                },
                new Currency
                {
                    Id = new Guid("550e8400-e29b-41d4-a716-446655440000"),
                    Code = "EUR",
                    Name = "Euro",
                    Symbol = "€",
                    CreatedAtUtc = SeedDate,
                    CreatedBy = createdBy
                }
            );
        }

        private static readonly DateTime SeedDate = DateTime.SpecifyKind(new DateTime(2026, 02, 20), DateTimeKind.Utc);

        DbSet<HedgeContract> HedgeContracts => Set<HedgeContract>();
        DbSet<Material> Materials => Set<Material>();
        DbSet<MaterialPriceHistory> MaterialPriceHistories => Set<MaterialPriceHistory>();
        DbSet<Organization> Organizations => Set<Organization>();
        DbSet<UserOrganizationMembership> Memberships => Set<UserOrganizationMembership>();
        DbSet<Project> Projects => Set<Project>();
        DbSet<Role> Roles => Set<Role>();
        DbSet<User> Users => Set<User>();
        DbSet<UserRole> UserRoles => Set<UserRole>();
        DbSet<DomainRule> DomainRules => Set<DomainRule>();
        DbSet<Currency> Currencies => Set<Currency>();
        DbSet<GlobalSettings> GlobalSettings => Set<GlobalSettings>();
    }
}
