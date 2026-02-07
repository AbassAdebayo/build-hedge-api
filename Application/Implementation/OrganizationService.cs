using Application.DTOs;
using Application.DTOs.Auth;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Application.Implementation
{
    public class OrganizationService(IUserRepository userRepository, ILogger<OrganizationService> logger,
        UserManager<User> userManager, IIdentityService identityService, IOrganizationRepository organizationRepository,
        IUserOrganizationMembershipRepository membershipRepository, IRoleRepository roleRepository, 
        IMailService mailService, IUnitOfWork unitOfWork) : IOrganizationService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ILogger<OrganizationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IUserOrganizationMembershipRepository _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
        private readonly IOrganizationRepository _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        private readonly IMailService _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));

        public async Task<AuthResponse> AddExistingAdminToOrganizationAsync(Guid userId, AddOrganizationToExistingAdminRequest request)
        {
            var existingUserAdmin = await _userRepository.Get<User>(u => u.Id == userId);
            if (existingUserAdmin is null)
                return new AuthResponse("Admin not found. Please check the user ID and try again.", false);

            var existingOrg = await _organizationRepository.Any<Organization>(org => org.BusinessName == request.BusinessName);
            if (existingOrg)
                return new AuthResponse("This organization is already registered on BuildHedge.", false);

            // Using the execution strategy to handle potential transient failures during the transaction
            var strategy = _unitOfWork.CreateExecutionStrategy();

            AuthResponse response = await strategy.ExecuteAsync(async () =>
            {

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var newOrganization = new Organization
                    {
                        BusinessName = request.BusinessName,
                        SubscriptionPlan = request.SubscriptionPlan,
                        TaxId = request.TaxId,
                        IsActive = false,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    var addOrg = await _organizationRepository.Add(newOrganization);
                    if (addOrg is null)
                        return new AuthResponse("Failed to create organization. Please try again.", false);
                    await _unitOfWork.SaveChangesAsync();

                    var membership = new UserOrganizationMembership
                    {
                        UserId = existingUserAdmin.Id,
                        OrganizationId = newOrganization.Id,
                        RoleInOrganization = "Hedge_Admin",
                        JoinedAtUtc = DateTime.UtcNow
                    };
                    var addMembership = await _membershipRepository.Add(membership);
                    if (addMembership is null)
                        return new AuthResponse("User couldn't be added to organization", false);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit the transaction only after both operations succeed
                    await transaction.CommitAsync();
                    return new AuthResponse("Registration successful! Please check your email to verify your account.", true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering organization");
                    await transaction.RollbackAsync();
                    return new AuthResponse("An error occurred while registering the organization.", false);
                }
                
            });
            return response;
        }            

        public async Task<AuthResponse> RegisterOrganizationAsync(RegisterOrganizationRequestModel request)
        {
            var checkEmailValidity = await _identityService.IsEmailAuthorizedForOrganizationSetup(request.AdminEmail);
            if (!checkEmailValidity)
                return new AuthResponse("The provided email is not authorized. Please use a valid business email or contact support.", false);

            var existingOrg = await _organizationRepository
                .Get<Organization>(org => org.BusinessName == request.BusinessName);

            if(existingOrg is not null)
                return new AuthResponse("This organization is already registered on BuildHedge.", false);   

            var existingAdminUser = await _userManager.FindByEmailAsync(request.AdminEmail);
            if(existingAdminUser is not null)
                return new AuthResponse("A user with this email already exists. Please use a different email for the organization admin.", false);

            if(existingOrg is not null && existingAdminUser is not null)
            {
                var alreadyMember = await _membershipRepository
                .Any<UserOrganizationMembership>(uorg => uorg.OrganizationId == existingOrg.Id && uorg.UserId == existingOrg.Id);

                if (alreadyMember)
                {
                    return new AuthResponse("You are already a member of this organization. Please log in.", false);
                }
                else
                {
                    return new AuthResponse("This organization is already registered. If you believe this is an error, contact support.", false);
                }
                    
            }

            var strategy = _unitOfWork.CreateExecutionStrategy();

            AuthResponse response = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var hashSalt = _identityService.GenerateSalt();
                    var passwordHash = _identityService.GetPasswordHash(request.AdminPassword, hashSalt);

                    var adminUser = new User
                    {
                        FirstName = request.AdminFirstName,
                        LastName = request.AdminLastName,
                        Email = request.AdminEmail,
                        PhoneNumber = request.AdminPhoneNumber,
                        ProfilePictureUrl = request.ProfilePictureUrl,
                        PasswordHash = passwordHash,
                        EmailConfirmed = false,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    var identityResult = await _userManager.CreateAsync(adminUser);
                    if (!identityResult.Succeeded)
                    {
                        var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                        return new AuthResponse($"Account creation failed: {errors}", false);
                    }

                    var organization = new Organization
                    {
                        BusinessName = request.BusinessName,
                        SubscriptionPlan = request.SubscriptionPlan,
                        TaxId = request.TaxId,
                        IsActive = false,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    var addOrg = await _organizationRepository.Add(organization);
                    if (addOrg is null)
                        return new AuthResponse("Failed to create organization. Please try again.", false);
                    await _unitOfWork.SaveChangesAsync();

                    var adminRole = await _roleRepository.Get<Role>(r => r.Name == "Hedge_Admin");
                    if (adminRole is null)
                        return new AuthResponse("Role not found. Please contact support.", false);

                    var membership = new UserOrganizationMembership
                    {
                        UserId = adminUser.Id,
                        OrganizationId = organization.Id,
                        RoleInOrganization = adminRole.Name,
                        JoinedAtUtc = DateTime.UtcNow
                    };

                    var addMembership = await _membershipRepository.Add(membership);
                    if (addMembership is null)
                        return new AuthResponse("User couldn't be added to organization", false);
                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var token = _identityService.GenerateToken(adminUser, organization.Id);

                    // Send verification mail
                    try
                    {
                      await _mailService.SendVerificationMail(adminUser.Email, organization.BusinessName, token);
                       
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send verification email to user with Email {Email}", adminUser.Email);
                        return new AuthResponse($"User created, but unable to Send Verification Email to user with Email {adminUser.Email}", false);
                    }

                    return new AuthResponse("Registration successful! Please check your email to verify your account.", true);


                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering organization");
                    await transaction.RollbackAsync();
                    return new AuthResponse("An error occurred while registering the organization.", false);
                }

            });

            return response;

        }
    }
}
