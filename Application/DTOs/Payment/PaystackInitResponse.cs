using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Application.DTOs.Payment
{
    public record PaystackInitResponse
    (
        bool Status,
        string Message,
        PaystackInitData Data
    );

    public record PaystackInitData(
        [property: JsonProperty("authorization_url")] string AuthorizationUrl,
        [property: JsonProperty("access_code")] string AccessCode, 
        [property: JsonProperty("reference")] string Reference
      );

    public record PaystackEvent(
    
        string Event,
        PaystackData Data
    );

    public record PaystackData(
    
        long Id,
        string Status,
        string Reference,
        int Amount,
        string Currency,
        [property: JsonProperty("paid_at")] DateTime PaidAt,
        PaystackMetadata Metadata,
        PaystackCustomer Customer
    );

    public record PaystackMetadata(
    
        [property: JsonProperty("invoice_id")] string InvoiceId,

        [property: JsonProperty("org_id")] string OrgId
    );

    public record PaystackCustomer(
    
       string Email
    );
}
