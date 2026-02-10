using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public record LoginResponseData(
        string Token, 
        Guid UserId, 
        string FullName, 
        bool IsVerified, 
        Guid CurrentOrganizationId, 
        IEnumerable<object> AvailableOrganizations, 
        string Role

     );
    

    public record LoginResponseModel(string Message, bool Status, LoginResponseData Data) : BaseResponse(Message, Status);

    public record LoginRequestModel(string Email, string Password);
    

}
