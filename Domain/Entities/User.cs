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
        public bool IsVerified { get; set; } = false;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserOrganizationMembership> Memberships { get; set; } = new List<UserOrganizationMembership>();


        public void ChangePassword(string newPasswordHash, string newHashSalt)
        {
            PasswordHash = newPasswordHash;
            HashSalt = newHashSalt;
        }
         public void UpdateProfile(string firstName, string lastName, string phoneNumber, string? profilePictureUrl)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            if (!string.IsNullOrEmpty(profilePictureUrl))
            {
                ProfilePictureUrl = profilePictureUrl;
            }
        }
    }
}
