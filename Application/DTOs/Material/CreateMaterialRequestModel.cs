using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Material
{
    public record CreateMaterialRequestModel(
        string Name,
        string TickerSymbol,
        string Unit,
        string? MetadataJson
        );
    
}
