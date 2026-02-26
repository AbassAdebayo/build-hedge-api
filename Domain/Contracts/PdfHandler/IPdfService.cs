using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.PdfHandler
{
    public interface IPdfService
    {
        Task<byte[]> GenerateInvoicePdf(Organization org, BillingStatement statement, List<HedgeContract> hedges);
    }
}
