using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public record RegisterOrganizationRequestModel(
        string BusinessName,
        string SubscriptionPlan,
        string? TaxId,
        string AdminEmail,
        string AdminFirstName,
        string AdminLastName,
        string AdminPhoneNumber,
        string ProfilePictureUrl,
        string AdminPassword,
        string AdminConfirmPassword,
        Guid CurrencyId
    );

    public record AddOrganizationToExistingAdminRequest(
        string BusinessName,
        string SubscriptionPlan,
        string? TaxId,
        Guid CurrencyId
    );

}
