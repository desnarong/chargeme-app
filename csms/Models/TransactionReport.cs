using csms.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace csms.Models
{
    public class TransactionReport : IDocument
    {
        private List<TransactionData> Model { get; }
        private TblCompany company = new TblCompany();
        private string StartDate { get; }
        private string EndDate { get; }
        private double total = 0.0, totalcharge = 0.0;
        public TransactionReport(List<TransactionData> model, TblCompany station, string startdate, string enddate)
        {
            Model = model;
            StartDate = startdate;
            EndDate = enddate;
            company = station;
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(25);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }
        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(14).SemiBold().FontFamily("CordiaUPC");
            var headertitleStyle = TextStyle.Default.FontSize(18).ExtraBold().FontFamily("CordiaUPC");

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignCenter().PaddingLeft(75).Text($"{company.FName}").Style(titleStyle);

                    column.Item().AlignCenter().PaddingLeft(75).Text($"{company.FAddress} {company.FCity} {company.FProvince}").Style(titleStyle);

                    column.Item().AlignCenter().PaddingLeft(75).Text($"รายงานรายละเอียดการชาร์จรถยนต์ไฟฟ้า").Style(headertitleStyle);
                });

               // row.ConstantItem(75).Height(75).Image(company.im);
            });
        }
        void ComposeFooter(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(12).FontFamily("CordiaUPC");
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span("/");
                        x.TotalPages();
                        x.DefaultTextStyle(titleStyle);
                    });
                    column.Item().AlignRight().Text($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("th-TH"))}").Style(titleStyle);
                });

            });
            
        }
        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                //column.Spacing(5);
                column.Item().Text($"ระหว่างวันที่ {StartDate} - {EndDate}").FontFamily("CordiaUPC");
                column.Spacing(5);
                column.Item().Element(ComposeTableDetail);
            });
        }
        void ComposeTableDetail(IContainer container)
        {
            var headerStyle = TextStyle.Default.FontSize(12).FontFamily("CordiaUPC");
            var titleStyle = TextStyle.Default.FontSize(12).FontFamily("CordiaUPC");

            container.Table(table =>
            {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(60);
                    columns.RelativeColumn(6);
                    columns.RelativeColumn(7);
                    columns.RelativeColumn(8);
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("บัตร RFID");
                    header.Cell().Element(CellStyle).Text("ทะเบียนรถ");
                    header.Cell().Element(CellStyle).Text("เครื่องชาร์จ");
                    header.Cell().Element(CellStyle).Text("หัวชาร์จ");
                    header.Cell().Element(CellStyle).Text("วัน/เวลา");
                    header.Cell().Element(CellStyle).Text("ระยะเวลา");
                    header.Cell().Element(CellStyle).Text("หน่วย (kWh)");
                    header.Cell().Element(CellStyleAlignRight).Text("จำนวนเงิน");
                    header.Cell().Element(CellStyle).Text("รายการ");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.FontSize(12).FontFamily("CordiaUPC"))
                        .BorderHorizontal(1).BorderColor(Colors.Grey.Darken2).AlignCenter();
                    }
                    static IContainer CellStyleAlignRight(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.FontSize(12).FontFamily("CordiaUPC"))
                        .BorderHorizontal(1).BorderColor(Colors.Grey.Darken2).AlignRight();
                    }
                });

                

                foreach (var item in Model)
                {
                    var station = StationInfoModel.GetStationInfo(company.FId);
                    var holidays = StationInfoModel.GetHolidays(station.FId);
                    var _holidays = holidays.Where(x => x.FDay == item.StartTime);
                    var chargesum = 0.0;
                    if (_holidays.Any())
                    {
                        chargesum = (Convert.ToDouble(station.FOffpeak ?? 0) * Convert.ToDouble(item.ChargeSum));
                    }
                    else
                    {
                        if ((item.StartTime ?? DateTime.MinValue).DayOfWeek == DayOfWeek.Sunday || (item.StartTime ?? DateTime.MinValue).DayOfWeek == DayOfWeek.Saturday)
                        {
                            chargesum = (Convert.ToDouble(station.FOffpeak ?? 0) * Convert.ToDouble(item.ChargeSum));
                        }
                        else
                        {
                            chargesum = (Convert.ToDouble(station.FOnpeak ?? 0) * Convert.ToDouble(item.ChargeSum));
                        }
                    }

                    table.Cell().AlignCenter().Text(item.StartTagId).Style(titleStyle);
                    if (string.IsNullOrEmpty(item.PlateNo))
                        table.Cell().AlignCenter().Text("");
                    else
                        table.Cell().AlignCenter().Text(item.PlateNo.Substring(0, (item.PlateNo.Length > 18 ? 18 : item.PlateNo.Length))).Style(titleStyle);
                    table.Cell().AlignCenter().Text(item.ChargerCode).Style(titleStyle);
                    table.Cell().AlignCenter().Text($"{item.ConnectorNo}").Style(titleStyle);
                    table.Cell().AlignCenter().Text((item.StartTime ?? DateTime.MinValue).ToString("dd/MM/yyyy HH:mm", new System.Globalization.CultureInfo("th-TH"))).Style(titleStyle);
                    table.Cell().AlignCenter().Text($"{item.UsedTime}").Style(titleStyle);
                    table.Cell().AlignCenter().Text(item.ChargeSum).Style(titleStyle);
                    table.Cell().AlignRight().Text($"{chargesum.ToString("#,0.00")}").Style(titleStyle);
                    table.Cell().AlignCenter().Text($"{item.TransactionId}").Style(titleStyle);

                    total += chargesum;
                    totalcharge += Convert.ToDouble(item.ChargeSum);
                }

                table.Cell().ColumnSpan(5).Text("");
                table.Cell().Element(CellStyleAlignCenter).Text("รวม");
                table.Cell().Element(CellStyleAlignCenter).Text($"{totalcharge.ToString("#,0.00#")}");
                table.Cell().Element(CellStyleAlignRight).Text($"{total.ToString("#,0.00")}");

                table.Cell().ColumnSpan(5).Text("");
                table.Cell().ColumnSpan(3).PaddingTop(3).BorderTop(1).BorderColor(Colors.Grey.Darken2).Text("");
                table.Cell().Text("");
                static IContainer CellStyleAlignRight(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.FontSize(13).FontFamily("CordiaUPC"))
                    .BorderHorizontal(1).BorderColor(Colors.Grey.Darken2).AlignRight();
                }
                static IContainer CellStyleAlignCenter(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.FontSize(13).FontFamily("CordiaUPC"))
                    .BorderHorizontal(1).BorderColor(Colors.Grey.Darken2).AlignCenter();
                }
            });
        }
    }
}
