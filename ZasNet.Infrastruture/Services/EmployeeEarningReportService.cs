using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Services;

namespace ZasNet.Infrastruture.Services;

/// <summary>
/// Сервис для генерации отчетов по заработкам сотрудников
/// </summary>
public class EmployeeEarningReportService : IEmployeeEarningReportService
{
    public async Task<byte[]> GenerateReportPdfAsync(List<EmployeeEarningByFilterDto> data, CancellationToken cancellationToken = default)
    {
        // Настройка лицензии QuestPDF (Community License)
        QuestPDF.Settings.License = LicenseType.Community;

        if (data == null || !data.Any())
        {
            throw new ArgumentException("Список данных для отчета не может быть пустым", nameof(data));
        }

        // Генерируем Excel файл
        byte[] excelBytes = await Task.Run(() => GenerateExcelFile(data), cancellationToken);

        // Конвертируем Excel в PDF
        byte[] pdfBytes = await Task.Run(() => ConvertExcelToPdf(data), cancellationToken);

        return pdfBytes;
    }

    public string GenerateFileName(string employeeName, int month, int year)
    {
        var monthName = GetMonthName(month);
        return $"Отчет_{employeeName}_{monthName}_{year}.pdf";
    }

    private byte[] GenerateExcelFile(List<EmployeeEarningByFilterDto> dataList)
    {
        // Создаем новый Excel файл
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Отчет");

        // Заполняем данными
        FillWorksheetWithData(worksheet, dataList);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void FillWorksheetWithData(IXLWorksheet worksheet, List<EmployeeEarningByFilterDto> dataList)
    {
        if (!dataList.Any()) return;

        var firstRecord = dataList.First();
        
        // Заголовок отчета
        var headerCell = worksheet.Cell("A1");
        headerCell.Value = "Отчет по заработной плате";
        headerCell.Style.Font.Bold = true;
        headerCell.Style.Font.FontSize = 16;
        
        worksheet.Cell("A2").Value = $"Сотрудник: {firstRecord.Employee.Name}";
        
        // Определяем период по всем записям
        var minDate = dataList.Min(d => d.OrderDateStart);
        var maxDate = dataList.Max(d => d.OrderDateEnd);
        worksheet.Cell("A3").Value = $"Период: {minDate:dd.MM.yyyy} - {maxDate:dd.MM.yyyy}";
        
        // Начинаем заполнение таблицы с данными
        int currentRow = 5;
        
        // Заголовки таблицы
        worksheet.Cell(currentRow, 1).Value = "ID заказа";
        worksheet.Cell(currentRow, 2).Value = "Клиент";
        worksheet.Cell(currentRow, 3).Value = "Услуга";
        worksheet.Cell(currentRow, 4).Value = "Объем работ";
        worksheet.Cell(currentRow, 5).Value = "Стоимость";
        worksheet.Cell(currentRow, 6).Value = "Процент %";
        worksheet.Cell(currentRow, 7).Value = "Заработок";
        
        // Форматируем заголовки таблицы
        var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
        currentRow++;
        
        // Заполняем данными
        decimal totalEarning = 0;
        foreach (var data in dataList)
        {
            worksheet.Cell(currentRow, 1).Value = data.OrderId;
            worksheet.Cell(currentRow, 2).Value = data.Client;
            worksheet.Cell(currentRow, 3).Value = data.ServiceName;
            
            // Форматируем объем работ
            var volumeCell = worksheet.Cell(currentRow, 4);
            volumeCell.Value = data.TotalVolume;
            volumeCell.Style.NumberFormat.Format = "#,##0.00";
            
            // Форматируем денежные значения
            var priceCell = worksheet.Cell(currentRow, 5);
            priceCell.Value = data.ServiceTotalPrice;
            priceCell.Style.NumberFormat.Format = "#,##0.00 ₽";
            
            var percentCell = worksheet.Cell(currentRow, 6);
            percentCell.Value = data.EmployeeEarningDto.ServiceEmployeePrecent / 100; // Конвертируем в десятичную дробь для формата процента
            percentCell.Style.NumberFormat.Format = "0.00%";
            
            var earningCell = worksheet.Cell(currentRow, 7);
            earningCell.Value = data.EmployeeEarningDto.EmployeeEarning;
            earningCell.Style.NumberFormat.Format = "#,##0.00 ₽";
            
            totalEarning += data.EmployeeEarningDto.EmployeeEarning;
            currentRow++;
        }
        
        // Итого
        currentRow++;
        var totalLabelCell = worksheet.Cell(currentRow, 6);
        totalLabelCell.Value = "ИТОГО:";
        totalLabelCell.Style.Font.Bold = true;
        totalLabelCell.Style.Fill.BackgroundColor = XLColor.LightGray;
        totalLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        
        var totalValueCell = worksheet.Cell(currentRow, 7);
        totalValueCell.Value = totalEarning;
        totalValueCell.Style.Font.Bold = true;
        totalValueCell.Style.Font.FontSize = 14;
        totalValueCell.Style.NumberFormat.Format = "#,##0.00 ₽";
        totalValueCell.Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Автоподбор ширины колонок
        worksheet.Columns().AdjustToContents();
    }

    private byte[] ConvertExcelToPdf(List<EmployeeEarningByFilterDto> dataList)
    {
        if (!dataList.Any())
        {
            throw new ArgumentException("Список данных не может быть пустым");
        }

        var firstRecord = dataList.First();
        var minDate = dataList.Min(d => d.OrderDateStart);
        var maxDate = dataList.Max(d => d.OrderDateEnd);
        var totalEarning = dataList.Sum(d => d.EmployeeEarningDto.EmployeeEarning);

        // Создаем PDF документ с использованием QuestPDF
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Text($"Отчет по заработной плате")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                        
                        column.Item().Text(text =>
                        {
                            text.Span("Сотрудник: ").SemiBold();
                            text.Span(firstRecord.Employee.Name);
                        });

                        column.Item().Text(text =>
                        {
                            text.Span("Период: ").SemiBold();
                            text.Span($"{minDate:dd.MM.yyyy} - {maxDate:dd.MM.yyyy}");
                        });
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(5);

                        // Таблица с данными
                        column.Item().Table(table =>
                        {
                            // Определение колонок
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // ID
                                columns.RelativeColumn(2);   // Клиент
                                columns.RelativeColumn(2);   // Услуга
                                columns.ConstantColumn(60);  // Объем
                                columns.ConstantColumn(70);  // Стоимость
                                columns.ConstantColumn(50);  // Процент
                                columns.ConstantColumn(70);  // Заработок
                            });

                            // Заголовки
                            table.Cell().Element(HeaderCellStyle).Text("ID").FontSize(9);
                            table.Cell().Element(HeaderCellStyle).Text("Клиент").FontSize(9);
                            table.Cell().Element(HeaderCellStyle).Text("Услуга").FontSize(9);
                            table.Cell().Element(HeaderCellStyle).Text("Объем").FontSize(9);
                            table.Cell().Element(HeaderCellStyle).Text("Стоимость").FontSize(9);
                            table.Cell().Element(HeaderCellStyle).Text("Процент").FontSize(9);
                            table.Cell().Element(HeaderCellStyle).Text("Заработок").FontSize(9);

                            // Данные
                            foreach (var data in dataList)
                            {
                                table.Cell().Element(DataCellStyle).Text(data.OrderId.ToString()).FontSize(8);
                                table.Cell().Element(DataCellStyle).Text(data.Client).FontSize(8);
                                table.Cell().Element(DataCellStyle).Text(data.ServiceName).FontSize(8);
                                table.Cell().Element(DataCellStyle).AlignRight().Text($"{data.TotalVolume:N2}").FontSize(8);
                                table.Cell().Element(DataCellStyle).AlignRight().Text($"{data.ServiceTotalPrice:N2}").FontSize(8);
                                table.Cell().Element(DataCellStyle).AlignRight().Text($"{data.EmployeeEarningDto.ServiceEmployeePrecent:N2}%").FontSize(8);
                                table.Cell().Element(DataCellStyle).AlignRight().Text($"{data.EmployeeEarningDto.EmployeeEarning:N2}").FontSize(8);
                            }

                            // Итоговая строка
                            table.Cell().ColumnSpan(6).Element(TotalCellStyle).AlignRight().Text("ИТОГО:").Bold();
                            table.Cell().Element(TotalCellStyle).AlignRight().Text($"{totalEarning:N2} ₽").Bold().FontSize(11);
                        });

                        column.Item().PaddingTop(10).Text($"Итоговая сумма заработка: {totalEarning:N2} ₽")
                            .SemiBold().FontSize(14).FontColor(Colors.Green.Darken2);
                    });
            });
        })
        .GeneratePdf();

        return pdfBytes;
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Darken2)
            .Background(Colors.Grey.Lighten3)
            .Padding(5);
    }

    private static IContainer DataCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }

    private static IContainer TotalCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Darken2)
            .Background(Colors.Grey.Lighten4)
            .Padding(5);
    }

    private string GetMonthName(int month)
    {
        return month switch
        {
            1 => "Январь",
            2 => "Февраль",
            3 => "Март",
            4 => "Апрель",
            5 => "Май",
            6 => "Июнь",
            7 => "Июль",
            8 => "Август",
            9 => "Сентябрь",
            10 => "Октябрь",
            11 => "Ноябрь",
            12 => "Декабрь",
            _ => "Неизвестно"
        };
    }
}

