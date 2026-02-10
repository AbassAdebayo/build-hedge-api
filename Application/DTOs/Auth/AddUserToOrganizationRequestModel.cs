using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public record AddUserToOrganizationRequestModel(
        string Email,
        string FirstName,
        string LastName,
        string PhoneNumber,
        string ProfilePictureUrl,
        Guid OrganizationId,
        Guid RoleId

        );
    
}
