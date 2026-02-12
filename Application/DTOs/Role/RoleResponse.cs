using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Role
{
    public record RoleResponse(Guid id, string name, string description);
    
}
