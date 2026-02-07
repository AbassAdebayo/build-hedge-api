using Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Interfaces.Identity
{
    public interface IIdentityService
    {
        public Task<bool> IsEmailAuthorizedForOrganizationSetup(string email);
        string GetUserIdentity();
        //string GenerateToken(User user, IEnumerable<string> roles);
        string GenerateToken(User user, Guid selectedOrgId);
        public IEnumerable<Claim> ValidateToken(string jwtToken);
        JwtSecurityToken GetClaims(string token);
        string GetClaimValue(string type);
        string GenerateSalt();
        public string GetPasswordHash(string password, string salt = null!);
        Task<List<string>> GetRolesAsync(User user);
        public Task<User> GetLoggedInUser();

        //bool CheckPasswordAsync(User user, string password);
        //Task<User> FindByNameAsync(string userName);
        //Task<User> FindUserAsync(string userName);
    }
}
