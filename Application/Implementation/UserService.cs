using Application.DTOs.Auth;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.MailingServices;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Application.Implementation
{
    public class UserService(IUserRepository userRepository, ILogger<UserService> logger,
        UserManager<User> userManager, IIdentityService identityService,
        IUserOrganizationMembershipRepository membershipRepository, IRoleRepository roleRepository,
        IMailService mailService, IOrganizationRepository organizationRepository, 
        IConfiguration configuration, IUnitOfWork unitOfWork) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ILogger<UserService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IUserOrganizationMembershipRepository _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
        private readonly IMailService _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        private readonly IOrganizationRepository _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        public async Task<AuthResponse> InviteUserToOrganizationAsync(Guid adminUserId, Guid organizationId, AddUserToOrganizationRequestModel request)
        {
            var isAdminMember = await _membershipRepository.Any<UserOrganizationMembership>(m => m.OrganizationId == organizationId && m.UserId == adminUserId);

            if (!isAdminMember)
                return new AuthResponse("This Admin is not a member of this organization.", false);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if(existingUser is not null)
            {
                var alreadyMember = await _membershipRepository.Any<UserOrganizationMembership>(m => m.OrganizationId == organizationId && m.UserId == existingUser.Id);
                if (alreadyMember)
                    return new AuthResponse("This user is already a member of this organization.", false);
            }

            var strategy = _unitOfWork.CreateExecutionStrategy();

            AuthResponse response = await strategy.ExecuteAsync(async () =>
            {
                var creator = await _userRepository.Get<User>(u => u.Id == adminUserId);
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var hashSalt = _identityService.GenerateSalt();
                    var passwordHash = _identityService.GetPasswordHash("1234", hashSalt);
                    var user = new User
                    {
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        PhoneNumber = request.PhoneNumber,
                        ProfilePictureUrl = request.ProfilePictureUrl,
                        PasswordHash = passwordHash,
                        HashSalt = hashSalt,
                        CreatedBy = creator.FirstName
                    };

                    var identityResult = await _userManager.CreateAsync(user);
                    if (!identityResult.Succeeded)
                    {
                        var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                        return new AuthResponse($"Account creation failed: {errors}", false);
                    }

                    var role = await _roleRepository.Get<Role>(r => r.Id == request.RoleId);

                    var allowedRoles = new List<string> { "Hedge_Editor", "Hedge_Viewer" };

                    if (!allowedRoles.Contains(role.Name))
                    {
                        return new AuthResponse("Invalid role. Only Hedge_Editor and Hedge_Viewer roles are allowed for organization members.", false);
                    }

                    var membership = new UserOrganizationMembership
                    {
                        UserId = user.Id,
                        OrganizationId = organizationId,
                        RoleInOrganization = role.Name,
                        JoinedAtUtc = DateTime.UtcNow
                    };

                    var addMembership = await _membershipRepository.Add(membership);
                    if (addMembership is null)
                        return new AuthResponse("User couldn't be added to organization", false);
                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var token = _identityService.GenerateToken(user, organizationId);

                    // Send verification mail
                    try
                    {
                        var emailSent = await _mailService.SendInvitationMail(user.Email, user.FirstName, token, membership.RoleInOrganization);
                        if (!emailSent)
                            return new AuthResponse("Unable to send Invitation mail", false);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send Invitation email to user with Email {user.Email}", user.Email);
                        return new AuthResponse($"User created, but unable to Send Invitation Email to user with Email {user.Email}", false);
                    }

                    return new AuthResponse("Registration successful! Please check your email to accept invite.", true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user");
                    await transaction.RollbackAsync();
                    return new AuthResponse("An error occurred while creating user.", false);
                }
            });

            return response;

        }

        public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequestModel request)
        {
            var user = await _userManager.FindByEmailAsync(request.email);
            if (user is null)
                return new AuthResponse("User with this email doesn't exist.", false);

            var token = _identityService.GenerateToken(user, Guid.Empty);

            var resetLink = $"{_configuration.GetSection("BuildHedgeUrls:BaseUrl").Value}/{_configuration.GetSection("BuildHedgeUrls:PasswordReset").Value}{token}";

            // Send reset password link mail
            var sent = await _mailService.SendForgotPasswordMail(user.Email, user.FirstName, resetLink);
            if(!sent)
                return new AuthResponse("Unable to send reset password mail. Please try again later.", false);

            return new AuthResponse("Password reset link has been sent to your email.", true);


        }

        public async Task<AuthResponse> ResendVerificationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Prevent email harvesting by returning a generic message regardless of whether the user exists or not
            if (user is null)
                return new AuthResponse("If an account exists, an email has been sent.", true);

            if (user.IsVerified)
                return new AuthResponse("This account is already verified. Please log in.", false);

            var membership = await _membershipRepository.Get<UserOrganizationMembership>(m => m.UserId == user.Id); 
            if (membership is null) return new AuthResponse("User is not associated with any organization. Please contact support.", false);

            var verificationToken = _identityService.GenerateToken(user, membership.OrganizationId);

            // Send verification mail
            try
            {
                var emailSent = await _mailService.SendInvitationMail(user.Email, user.FirstName, verificationToken, membership.RoleInOrganization);
                if (!emailSent)
                    return new AuthResponse("Unable to send Invitation mail", false);

                return new AuthResponse("Verification email resent successfully! Please check your email to verify your account.", true);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send Invitation email to user with Email {user.Email}", user.Email);
                return new AuthResponse($"User created, but unable to Send Invitation Email to user with Email {user.Email}", false);
            }


        }

        public async Task<AuthResponse> ResetPasswordAsync(string token, ResetPasswordRequestModel request)
        {
            var claims = _identityService.ValidateToken(token);
            var email = claims.SingleOrDefault(c => c.Type == "email")!.Value;

            var user = await _userManager.FindByEmailAsync(email);
            if(user is null)
                return new AuthResponse("User not found.", false);

            var hashSalt = _identityService.GenerateSalt();
            var passwordHash = _identityService.GetPasswordHash(request.password, hashSalt);

            user.ChangePassword(passwordHash, hashSalt);
            user.UpdatedAtUtc = DateTime.UtcNow;

            await _userRepository.Update(user);
            return await _unitOfWork.SaveChangesAsync() > 0
                ? new AuthResponse("Password reset successful! Please login with your new password.", true)
                : new AuthResponse("An error occurred while resetting password. Please try again later.", false);

        }

        public async Task<AuthResponse> VerifyUserAsync(string token)
        {
            var claims = _identityService.ValidateToken(token);
            if (claims is null) return new AuthResponse("Unable to validate with the provided token", false);
            var email = claims.SingleOrDefault(c => c.Type == "email");
            if (email == null) return new AuthResponse("User Claims not valid", false);
            var user = await _userRepository.Get<User>(u => u.Email == email.Value);
            if (user == null) return new AuthResponse("Unable to find user", false);
            if (user.IsVerified) return new AuthResponse("User already Validated", false);

            var strategy = _unitOfWork.CreateExecutionStrategy();

            AuthResponse response = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var update = user.IsVerified = true;
                    user.UpdatedAtUtc = DateTime.UtcNow;
                    var userUpdate = await _userRepository.Update<User>(user);

                    var membership = await _membershipRepository.Get<UserOrganizationMembership>(m => m.UserId == user.Id);

                    string message = "User validated successfully! Kindly login to access your organization";

                    if (membership is not null && membership.RoleInOrganization == "Hedge_Admin")
                    {
                        var organization = await _organizationRepository.Get<Organization>(o => o.Id == membership.OrganizationId);
                        if(organization is null) return new AuthResponse("Unable to find organization for this user", false);
                        organization.IsActive = true;
                        organization.IsInTrial = true;
                        organization.TrialExpiryDate = DateTime.UtcNow.AddDays(14);
                        await _organizationRepository.Update<Organization>(organization);
                       
                        message = "Admin validated successfully! Kindly login to setup your organization";
                    }
                    
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new AuthResponse(message, true);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error validating user");
                    await transaction.RollbackAsync();
                    return new AuthResponse("An error occurred while validating user.", false);
                }

            });

            return response;

        }

        public async Task<AuthResponse<string>> SwitchOrganizationAsync(Guid userId, Guid organizationId)
        {
            var user = await _userRepository.Get<User>(u => u.Id == userId); 
            if (user is null) return new AuthResponse<string>("User not found.", false, null); 
            var membership = await _membershipRepository.Get<UserOrganizationMembership>(m => m.UserId == userId && m.OrganizationId == organizationId); 
            if (membership is null) return new AuthResponse<string>("User is not a member of this organization.", false, null); 

            var token = _identityService.GenerateToken(user, organizationId); 
            return new AuthResponse<string>("Organization switched successfully!", true, token);
        }
    }
}
