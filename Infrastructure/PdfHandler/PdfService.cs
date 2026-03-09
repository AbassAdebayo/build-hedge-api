using Domain.Contracts.PdfHandler;
using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.PdfHandler
{
    public class PdfService : IPdfService
    {
        //public async Task<byte[]> GenerateInvoicePdf(Organization org, BillingStatement statement, List<HedgeContract> hedges)
        //{
        //    QuestPDF.Settings.License = LicenseType.Community;

        //    var document = Document.Create(container =>
        //    {
        //        container.Page(page =>
        //        {
        //            page.Margin(50);
        //            page.Size(QuestPDF.Helpers.PageSizes.A4);

        //            // HEADER: Corporate Identity
        //            page.Header().Row(row =>
        //            {
        //                row.RelativeItem().Column(col =>
        //                {
        //                    col.Item().Text("BUILDHEDGE").FontSize(20).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
        //                    col.Item().Text($"Organization: {org.BusinessName}").FontSize(10);
        //                    col.Item().Text($"Billing Period: {DateTime.UtcNow.AddMonths(-1):MMMM yyyy}").FontSize(10);
        //                });

        //                row.RelativeItem().AlignRight().Column(col =>
        //                {
        //                    col.Item().Text("INVOICE").FontSize(25).Light();
        //                    col.Item().Text($"Invoice #: {statement.InvoiceNumber}").SemiBold();
        //                    col.Item().Text($"Due Date: {statement.DueDate:dd MMM yyyy}").FontColor(QuestPDF.Helpers.Colors.Red.Medium);
        //                });
        //            });

        //            // CONTENT: The Itemized Audit Trail
        //            page.Content().PaddingVertical(20).Column(col =>
        //            {
        //                col.Item().Table(table =>
        //                {
        //                    table.ColumnsDefinition(columns =>
        //                    {
        //                        columns.RelativeColumn(2); 
        //                        columns.RelativeColumn();
        //                        columns.RelativeColumn();
        //                        columns.RelativeColumn();
        //                        columns.RelativeColumn(); 
        //                    });

        //                    // Table Styles
        //                    static IContainer Block(IContainer container) => container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);

        //                    table.Header(header =>
        //                    {
        //                        header.Cell().Element(Block).Text("Material").SemiBold();
        //                        header.Cell().Element(Block).AlignRight().Text("Qty").SemiBold();
        //                        header.Cell().Element(Block).AlignRight().Text("Price").SemiBold();
        //                        header.Cell().Element(Block).AlignRight().Text("Premium").SemiBold();
        //                        header.Cell().Element(Block).AlignRight().Text("Overage").SemiBold();
        //                    });

        //                    foreach (var hedge in hedges)
        //                    {
        //                        table.Cell().Element(Block).Text(hedge.Material?.Name ?? "Unknown Material");
        //                        table.Cell().Element(Block).AlignRight().Text($"{hedge.Quantity:N0}");
        //                        table.Cell().Element(Block).AlignRight().Text($"{hedge.LockedPrice:N2}");
        //                        table.Cell().Element(Block).AlignRight().Text($"{hedge.PremiumFee:N2}");
        //                        table.Cell().Element(Block).AlignRight().Text($"{hedge.OverageFee:N2}");
        //                    }
        //                });

        //                // SUMMARY BOX: The Financial Aid Breakdown
        //                col.Item().PaddingTop(20).AlignRight().Column(c =>
        //                {
        //                    c.Item().Text($"Subscription Base Fee: {statement.SubscriptionBaseFee:N2}").FontSize(11);
        //                    c.Item().Text($"Total Risk Premiums: {statement.TotalPremiumFees:N2}").FontSize(11);
        //                    c.Item().Text($"Total Overage Fees: {statement.TotalOverageFees:N2}").FontSize(11);
        //                    c.Item().PaddingTop(5).Text($"Total Amount Due: {statement.TotalAmountDue:C}").FontSize(16).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
        //                });
        //            });

        //            // FOOTER: Compliance and Support
        //            page.Footer().AlignCenter().Column(f =>
        //            {
        //                f.Item().LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
        //                f.Item().PaddingTop(5).Text(x =>
        //                {
        //                    x.Span("For billing inquiries, contact ");
        //                    x.Span("billing@buildhedge.com").SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
        //                    x.Span(" • Page ");
        //                    x.CurrentPageNumber();
        //                });
        //            });
        //        });
        //    });

        //    return document.GeneratePdf();
        //}

        public async Task<byte[]> GenerateInvoicePdf(Organization org, BillingStatement statement, List<HedgeContract> hedges)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(QuestPDF.Helpers.PageSizes.A4);

                    // HEADER (Same as before, ensuring Org Currency is shown)
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            //col.Item().Text("BUILDHEDGE").FontSize(20).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                            col.Item().Image("C:\\Users\\HOD-SW\\Desktop\\Proj_Repos\\build-hedge-api\\Api\\wwwroot\\assets\\build_hedge_logo.png").UseOriginalImage();
                            col.Item().Text($"Organization: {org.BusinessName}").FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                            col.Item().Text($"Account Currency: {org.BaseCurrencyCode}").FontSize(9).Italic();
                        });
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("INVOICE").FontSize(25).Light();
                            col.Item().Text($"Invoice #: {statement.InvoiceNumber}").SemiBold();
                            col.Item().Text($"Due Date: {statement.DueDate:dd MMM yyyy}").FontColor(QuestPDF.Helpers.Colors.Red.Medium);
                        });
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1f);
                            });

                            static IContainer Block(IContainer container) => container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);

                            table.Header(header =>
                            {
                                header.Cell().Element(Block).Text("Material").SemiBold();
                                header.Cell().Element(Block).AlignRight().Text("Qty").SemiBold();
                                header.Cell().Element(Block).AlignRight().Text("Lock Price").SemiBold();
                                header.Cell().Element(Block).AlignRight().Text("Premium").SemiBold();
                                header.Cell().Element(Block).AlignRight().Text("Overage").SemiBold();
                            });

                            foreach (var hedge in hedges)
                            {
                                // Using the Currency used for that specific Material lock
                                var mCurrency = hedge.Currency.Code ?? org.BaseCurrencyCode;

                                table.Cell().Element(Block).Text(hedge.Material?.Name ?? "Unknown");
                                table.Cell().Element(Block).AlignRight().Text($"{hedge.Quantity:N0} {hedge.Material?.Unit}");

                                // Explicitly append currency code for transparency
                                table.Cell().Element(Block).AlignRight().Text($"{hedge.LockedPrice:N2} {mCurrency}");
                                table.Cell().Element(Block).AlignRight().Text($"{hedge.PremiumFee:N2} {org.BaseCurrencyCode}");
                                table.Cell().Element(Block).AlignRight().Text($"{hedge.OverageFee:N2} {org.BaseCurrencyCode}");
                            }
                        });

                        // SUMMARY BOX: Consolidated to Organization Base Currency
                        col.Item().PaddingTop(20).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Subscription Base Fee: {statement.SubscriptionBaseFee:N2} {org.BaseCurrencyCode}").FontSize(11);
                            c.Item().Text($"Total Risk Premiums: {statement.TotalPremiumFees:N2} {org.BaseCurrencyCode}").FontSize(11);
                            c.Item().Text($"Total Overage Fees: {statement.TotalOverageFees:N2} {org.BaseCurrencyCode}").FontSize(11);

                            // The Total should be prominent and clearly labelled with the Org's payment currency
                            c.Item().PaddingTop(5).Text(x => {
                                x.Span("Total Amount Due: ").FontSize(14);
                                x.Span($"{statement.TotalAmountDue:N2}").FontSize(18).SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                                x.Span($" {org.BaseCurrencyCode}").FontSize(14).SemiBold();
                            });
                        });
                    });
                    // FOOTER: Compliance and Support
                    page.Footer().AlignCenter().Column(f =>
                    {
                        f.Item().LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        f.Item().PaddingTop(5).Text(x =>
                        {
                            x.Span("For billing inquiries, contact ");
                            x.Span("billing@buildhedge.com").SemiBold().FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                            x.Span(" • Page ");
                            x.CurrentPageNumber();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
