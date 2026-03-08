using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using CRM.Application.Services.Order_Service;
using System.Net;
using System.Text.RegularExpressions;

namespace CRM.Application.Services.Email_Service
{
    public static class OrderPdfGenerator
    {
        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Replace some block-level tags with line breaks before removing tags.
            var withBreaks = Regex.Replace(html, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
            withBreaks = Regex.Replace(withBreaks, "</p\\s*>", "\n", RegexOptions.IgnoreCase);
            withBreaks = Regex.Replace(withBreaks, "</li\\s*>", "\n", RegexOptions.IgnoreCase);

            var noTags = Regex.Replace(withBreaks, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(noTags).Trim();
        }

        public static byte[] GenerateOrderRequestPdf(OrderViewModel order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("STOCKS FULFILLMENT REQUEST").FontSize(20).SemiBold().FontColor("#10b981");
                            column.Item().Text($"Order Reference: #{order.OrderNumber ?? order.Id.ToString()}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });

                        try
                        {
                            row.ConstantItem(100).AlignRight().Image(System.Net.WebRequest.Create("https://i.postimg.cc/W4N26c0T/mainlogo.jpg").GetResponse().GetResponseStream());
                        }
                        catch
                        {
                            // If logo fails, we just don't show it instead of crashing the whole process
                            row.ConstantItem(100).AlignRight().Text("AGORA FOOD").FontSize(12).Bold().FontColor("#10b981");
                        }
                    });

                    page.Content().PaddingVertical(20).Column(x =>
                    {
                        x.Spacing(10);

                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ISSUED BY").SemiBold().FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text("Agora Food").SemiBold();
                                c.Item().Text("Vognmandsmarken 45, 2mf");
                                c.Item().Text("2100 Copenhagen, Denmark");
                                c.Item().Text("T: +45 60818181");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("REQUEST DATE").SemiBold().FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Text(System.DateTime.Now.ToString("dd MMM yyyy"));
                            });
                        });

                        x.Item().PaddingTop(20).Text("ITEM DETAILS").SemiBold().FontSize(12).Underline();

                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Product");
                                header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                                header.Cell().Element(CellStyle).AlignRight().Text("Unit Price");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var item in order.Items ?? new System.Collections.Generic.List<OrderItemViewModel>())
                            {
                                table.Cell().Element(CellStyle).Text(item.Name);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"${item.UnitPrice:F2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"${(item.UnitPrice * item.Quantity):F2}");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                }
                            }
                        });

                        var customerQueryText = StripHtml(order.CustomerQuery);

                        if (!string.IsNullOrWhiteSpace(customerQueryText))
                        {
                            x.Item().PaddingTop(20).Column(c =>
                            {
                                c.Item().Text("SPECIAL INSTRUCTIONS / CUSTOMER QUERY").SemiBold().FontSize(8).FontColor(Colors.Grey.Medium);
                                c.Item().Padding(10).Background(Colors.Grey.Lighten4).Text(customerQueryText).Italic();
                            });
                        }

                        x.Item().PaddingTop(40).Column(c =>
                        {
                            c.Item().Text("AUTHORIZED DISPATCH").SemiBold().FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().PaddingTop(5).Text("_______________________");
                            c.Item().Text("Mir Mohammad Faruk");
                            c.Item().Text("Founder & CEO");
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
