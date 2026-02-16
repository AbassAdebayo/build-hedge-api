using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Project
{
    public record CreateProjectRequestModel(
        string Name,
        string Description,
        decimal TotalBudget,
        DateTime EndDate
    );
    
}
