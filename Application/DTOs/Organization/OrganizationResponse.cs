using Domain.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Organization
{
    public record OrganizationResponse(Guid Id, string businessName);
    public record OrganizationDetailsResponse(Guid Id, string businessName, string? taxId, string subscriptionPlan, bool isActive, DateTime dateRegistered);

}
