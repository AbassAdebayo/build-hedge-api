using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record BaseResponse(string Message, bool Status);
    public record BaseResponse<T>(string Message, bool Status, T Data);
    public record BaseResponseList<T>(string Message, bool Status, List<T> Data);

}
