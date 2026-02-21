using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.GlobalSettings
{
    public record UpdateGlobalSettingsRequestModel(
        string Key,
        string NewValue
    );
    
}
