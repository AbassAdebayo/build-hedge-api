using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity
{
    public class IdentityService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IPasswordHasher<User> passwordHasher, BuildHedgeContext context, IUserRepository userRepository) : IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
        private readonly IPasswordHasher<User> _passwordHasher = passwordHasher ?? throw new ArgumentException(nameof(passwordHasher));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<bool> IsEmailAuthorizedForOrganizationSetup(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var emailLower = email.ToLower().Trim();
            var domain = emailLower.Split('@').Last();

            // 1. THE PRECISION CHECK (Your Developer Backdoor)
            // Check if the SPECIFIC email is whitelisted in the DB
            var specificEmailRule = await _context.Set<DomainRule>()
                .FirstOrDefaultAsync(d => d.DomainName == emailLower);

            if (specificEmailRule != null) return specificEmailRule.IsAllowed;

            // 2. THE DOMAIN CHECK
            // Check if the DOMAIN (e.g., gmail.com) is explicitly blocked/allowed
            var domainRule = await _context.Set<DomainRule>()
                .FirstOrDefaultAsync(d => d.DomainName == domain);

            if (domainRule != null) return domainRule.IsAllowed;

            // 3. THE FALLBACK (Production Logic)
            // If nothing is in the DB, block common public providers
            string[] publicProviders = { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com" };
            return !publicProviders.Contains(domain);
        }
        public string GetUniqueKey(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            using var crypto = RandomNumberGenerator.Create();
            byte[] data = new byte[size];
            crypto.GetBytes(data);
            /*using (var crypto = new RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }*/
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public Task<User> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task<User> FindUserAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }
            //var user = await _gateway.GetUserAsync(userName);
            var user = await _userRepository.Get<User>(u => u.Email == email);
            if (user == null)
            {
                return null;
            }
            //return user;
            return new User
            {
                Email = user.Email
            };
        }

        public string GenerateSalt()
        {
            using var crypto = RandomNumberGenerator.Create();
            byte[] buffer = new byte[10];
            crypto.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public string GenerateToken(User user, Guid selectedOrgId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtTokenSettings:TokenKey").Value));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

            var membership = _context.Set<UserOrganizationMembership>()
            .FirstOrDefault(m => m.UserId == user.Id && m.OrganizationId == selectedOrgId);

            var currentRole = membership?.RoleInOrganization ?? "Hedge_Viewer";

            IList<Claim> claims = new List<Claim>
                {
                   new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                   new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                   new Claim(JwtRegisteredClaimNames.Email, user.Email),
                   new Claim("CurrentOrgId", selectedOrgId.ToString()),
                   new Claim(ClaimTypes.Name, user.FirstName),
                   new Claim(ClaimTypes.Role, currentRole)
                };
             

            var token = new JwtSecurityToken("", "", claims,
            DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration.GetSection("JwtTokenSettings:TokenExpiryPeriod").Value)),
            signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtSecurityToken GetClaims(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                if (token.StartsWith("B"))
                {
                    token = token.Split(" ")[1];
                }
                var handler = new JwtSecurityTokenHandler();

                var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

                return decodedToken;
            }
            Console.WriteLine(token);
            return null;
        }

        public string GetClaimValue(string type)
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(type).Value;
        }

        public string GetPasswordHash(string password, string salt = null)
        {
            if (string.IsNullOrEmpty(salt))
            {
                return _passwordHasher.HashPassword(new User(), password);
            }
            return _passwordHasher.HashPassword(new User(), $"{password}{salt}");
        }

        public async Task<List<string>> GetRolesAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var roles = await _userRepository.GetUserAndRoles(user.Id);

            return roles.UserRoles.Select(role => role.Role.Name).ToList();
        }

        public string GetUserIdentity()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.UniqueName).Value!;
        }

        //private string HashPasswordAsync(string password)
        //{
        //    using (var md5Hash = MD5.Create())
        //    {
        //        var sourceBytes = Encoding.UTF8.GetBytes(password);
        //        var hashBytes = md5Hash.ComputeHash(sourceBytes);
        //        var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        //        return hash.ToLower();
        //    }
        //}



        public IEnumerable<Claim> ValidateToken(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtTokenSettings:TokenKey").Value!);
            try
            {
                // Set the validation parameters for the token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key.ToArray()),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidIssuer = _configuration.GetSection("JwtTokenSettings:TokenIssuer").Value,
                    ClockSkew = TimeSpan.Zero
                };

                // Validate the token and extract the claims
                var claimsPrincipal = tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);
                var claims = ((JwtSecurityToken)validatedToken).Claims;

                // Return the claims
                return claims;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public async Task<User> GetLoggedInUser()
        {
            // Get the current HttpContext
            var httpContext = _httpContextAccessor.HttpContext;

            // Check if a user is authenticated
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (httpContext.User.Identity.IsAuthenticated)
            {
                // Retrieve the user's unique identifier (e.g., user ID) from claims
                var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userRepository.Get<User>(u => u.Email == email);

                return user;



            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            // If no user is authenticated, return null
            throw new BadHttpRequestException("Unable to get logged in user");
        }

        
    }
}
