using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Project
{
    public record ProjectDetailsResponse(
        Guid Id,
        string Name,
        string Description,
        decimal TotalBudget,
        byte[] RowVersion,
        DateTime EstimatedCompletion,
        Guid OrganizationId
    );
    
}
