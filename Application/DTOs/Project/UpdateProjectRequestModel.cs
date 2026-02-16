using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.Project
{
    public record UpdateProjectRequestModel(

        string Name,
        string Description,
        decimal TotalBudget,
         byte[] RowVersion,
        DateTime EndDate
     );
    
}
