using CaseMngmt.Models.Companies;
using CaseMngmt.Models.Customers;
using CaseMngmt.Models.Invoices;
using CaseMngmt.Models.Orders;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CaseMngmt.Service.Invoices
{
    public class InvoicePdfService : IInvoicePdfService
    {
        private const string FontFamily = "MS Gothic";

        public byte[] GeneratePdf(Invoice invoice, Order order, Company company, Customer customer)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("請求書").FontSize(22).Bold();
                        col.Item().PaddingTop(5).Text($"請求書番号: {invoice.InvoiceNumber}");
                        col.Item().Text($"発行日: {invoice.IssueDate:yyyy年MM月dd日}");
                        if (invoice.DueDate.HasValue)
                        {
                            col.Item().Text($"お支払期限: {invoice.DueDate:yyyy年MM月dd日}");
                        }
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("請求先").Bold();
                                c.Item().Text($"{customer.Name} 御中");
                                c.Item().Text(FormatAddress(customer.PostCode1, customer.PostCode2, customer.StateProvince, customer.City, customer.Street, customer.BuildingName, customer.RoomNumber));
                                c.Item().Text($"TEL: {customer.PhoneNumber}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("請求元").Bold();
                                c.Item().Text(company.Name);
                                c.Item().Text(FormatAddress(company.PostCode1, company.PostCode2, company.StateProvince, company.City, company.Street, company.BuildingName, company.RoomNumber));
                                c.Item().Text($"TEL: {company.PhoneNumber}");
                            });
                        });

                        col.Item().PaddingTop(10).Text($"注文番号: {order.OrderNumber}（注文日: {order.OrderDate:yyyy/MM/dd}）");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("品目");
                                header.Cell().Element(HeaderCellStyle).Text("数量");
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("単価");
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("金額");
                            });

                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().Element(BodyCellStyle).Text(item.ProductNameRaw);
                                table.Cell().Element(BodyCellStyle).Text(item.Quantity.ToString("#,0.##"));
                                table.Cell().Element(BodyCellStyle).AlignRight().Text(item.UnitPrice.ToString("#,0"));
                                table.Cell().Element(BodyCellStyle).AlignRight().Text(item.LineAmount.ToString("#,0"));
                            }
                        });

                        col.Item().AlignRight().Column(c =>
                        {
                            c.Item().Text($"小計: {invoice.SubTotalAmount:#,0} 円");
                            c.Item().Text($"消費税: {invoice.TaxAmount:#,0} 円");
                            c.Item().PaddingTop(5).Text($"合計金額: {invoice.TotalAmount:#,0} 円").FontSize(13).Bold();
                        });
                    });

                    page.Footer().AlignCenter().Text("Powered by ITFreee").FontSize(8);
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
        }

        private static IContainer BodyCellStyle(IContainer container)
        {
            return container.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
        }

        private static string FormatAddress(string? postCode1, string? postCode2, string? state, string? city, string? street, string? building, string? room)
        {
            var postCode = string.IsNullOrEmpty(postCode1) ? "" : $"〒{postCode1}-{postCode2} ";
            return $"{postCode}{state}{city}{street}{building}{room}".Trim();
        }
    }
}
