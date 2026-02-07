using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class User : BaseEntity
    {
#pragma warning disable CS8618
        public  string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string? HashSalt { get; set; }
        public string? ProfilePictureUrl { get; set; }
#pragma warning restore CS8618
        public bool EmailConfirmed { get; set; } = false;
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserOrganizationMembership> Memberships { get; set; } = new List<UserOrganizationMembership>();
    }
}
