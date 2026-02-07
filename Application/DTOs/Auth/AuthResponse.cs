using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public record AuthResponse(string Message, bool Status, IEnumerable<string> Errors = null!): BaseResponse(Message, Status);
}
