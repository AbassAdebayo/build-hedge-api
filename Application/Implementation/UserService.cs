using Application.DTOs.Auth;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.MailingServices;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        IMailService mailService, IUnitOfWork unitOfWork) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ILogger<UserService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IUserOrganizationMembershipRepository _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
        private readonly IMailService _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        public async Task<AuthResponse> InviteUserToOrganizationAsync(Guid adminUserId, Guid organizationId, AddUserToOrganizationRequestModel request)
        {
            var isAdminMember = await _membershipRepository.Any<UserOrganizationMembership>(m => m.OrganizationId == request.OrganizationId && m.UserId == adminUserId);

            if (isAdminMember)
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

                    var token = _identityService.GenerateToken(user, request.OrganizationId);

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

    }
}
