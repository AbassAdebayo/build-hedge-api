using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    public class UserOrganizationMembership : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        [Required]
#pragma warning disable CS8618 
        public string RoleInOrganization { get; set; }
#pragma warning restore CS8618 
        public DateTime JoinedAtUtc { get; set; }

    }
}
