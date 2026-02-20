using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Material
{
    public record ListOfMaterialsResponse(
        Guid Id,
        string Name,
        string TickerSymbol,
        string Unit,
        string? MetadataJson,
        byte[]? RowVersion,
        string CreatedBy
        );
    
}
