using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Project
{
    public record ListOfProjectsResponse(
        Guid Id,
        string Name,
        string Description,
        decimal TotalBudget,
        DateTime EstimatedCompletion,
        byte[]? RowVersion,
        string Organization

        );
    
}
