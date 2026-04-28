using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using ManageMentSystem.ViewModels;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ManageMentSystem.Services.ExcelExportServices
{
    public class ExcelExportService : IExcelExportService
    {
        public ExcelExportService()
        {
            // إعداد الترخيص لـ EPPlus 8+
            // في الإصدارات الحديثة، يتم إعداد الترخيص مرة واحدة في بداية التطبيق
            // هذا الإعداد موجود في Program.cs
        }

        public byte[] ExportGeneric<T>(IEnumerable<T> data, string sheetName, Dictionary<string, Func<T, object>> columns, string title = "")
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);
                int startRow = 1;

                // RTL Direction
                worksheet.View.RightToLeft = true;

                // Title Section
                if (!string.IsNullOrEmpty(title))
                {
                    worksheet.Cells[1, 1, 1, columns.Count].Merge = true;
                    worksheet.Cells[1, 1].Value = title;
                    worksheet.Cells[1, 1].Style.Font.Size = 18;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(243, 244, 246)); // Gray-100
                    worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.FromArgb(31, 41, 55)); // Gray-800
                    worksheet.Row(1).Height = 40;
                    startRow = 2;
                }

                // Headers
                int col = 1;
                foreach (var header in columns.Keys)
                {
                    worksheet.Cells[startRow, col].Value = header;
                    col++;
                }

                // Style Headers
                var headerRange = worksheet.Cells[startRow, 1, startRow, columns.Count];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Name = "Segoe UI";
                headerRange.Style.Font.Size = 12;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81)); // Gray-700
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(107, 114, 128));
                worksheet.Row(startRow).Height = 25;

                // Data
                int row = startRow + 1;
                foreach (var item in data)
                {
                    col = 1;
                    foreach (var selector in columns.Values)
                    {
                        var value = selector(item);
                        var cell = worksheet.Cells[row, col];
                        cell.Value = value;
                        cell.Style.Font.Name = "Segoe UI";
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        // Basic format for dates/numbers
                        if (value is DateTime || value is DateTime?)
                        {
                            cell.Style.Numberformat.Format = "yyyy-MM-dd";
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        else if (value is decimal || value is double || value is float)
                        {
                            cell.Style.Numberformat.Format = "#,##0.00";
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Numbers LTR usually
                        }
                        else
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                        
                        col++;
                    }

                    // Alternating Row Colors (Zebra Striping)
                    if (row % 2 == 0)
                    {
                        var rowRange = worksheet.Cells[row, 1, row, columns.Count];
                        rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251)); // Gray-50
                    }

                    row++;
                }

                // AutoFit Columns with minimum width
                worksheet.Cells.AutoFitColumns(15);

                // Add Borders to Data
                if (data.Any())
                {
                    var dataRange = worksheet.Cells[startRow + 1, 1, row - 1, columns.Count];
                    dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Top.Color.SetColor(Color.FromArgb(209, 213, 219)); // Gray-300
                    dataRange.Style.Border.Bottom.Color.SetColor(Color.FromArgb(209, 213, 219));
                    dataRange.Style.Border.Left.Color.SetColor(Color.FromArgb(209, 213, 219));
                    dataRange.Style.Border.Right.Color.SetColor(Color.FromArgb(209, 213, 219));
                }

                return package.GetAsByteArray();
            }
        }

        public byte[] ExportSalesReport(SalesReportViewModel model)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("تقرير المبيعات");
                worksheet.View.RightToLeft = true; // RTL

                // 1. Title Styling
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Value = "تقرير المبيعات الشامل";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138)); // Dark Blue
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Row(1).Height = 45;

                // 2. Period Subtitle
                worksheet.Cells[2, 1, 2, 6].Merge = true;
                worksheet.Cells[2, 1].Value = GetPeriodText(model.FromDate, model.ToDate);
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);
                worksheet.Row(2).Height = 25;

                // 3. Summary Statistics (Cards Layout Simulation)
                int summaryStartRow = 4;
                
                // Card 1: Total Sales
                var card1Range = worksheet.Cells[summaryStartRow, 1];
                card1Range.Value = "إجمالي المبيعات";
                card1Range.Style.Font.Bold = true;
                card1Range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                card1Range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(239, 246, 255)); // Blue-50
                worksheet.Cells[summaryStartRow, 2].Value = model.TotalSales;
                worksheet.Cells[summaryStartRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Card 2: Total Revenue
                var card2Range = worksheet.Cells[summaryStartRow, 3];
                card2Range.Value = "إجمالي الإيرادات";
                card2Range.Style.Font.Bold = true;
                card2Range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                card2Range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(236, 253, 245)); // Emerald-50
                worksheet.Cells[summaryStartRow, 4].Value = model.TotalRevenue;
                worksheet.Cells[summaryStartRow, 4].Style.Numberformat.Format = "#,##0.00";

                // Card 3: Avg Sale
                var card3Range = worksheet.Cells[summaryStartRow, 5];
                card3Range.Value = "متوسط الفاتورة";
                card3Range.Style.Font.Bold = true;
                card3Range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                card3Range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 251, 235)); // Amber-50
                worksheet.Cells[summaryStartRow, 6].Value = model.AverageSaleValue;
                worksheet.Cells[summaryStartRow, 6].Style.Numberformat.Format = "#,##0.00";

                // Border for summary
                var summaryRange = worksheet.Cells[summaryStartRow, 1, summaryStartRow, 6];
                summaryRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                summaryRange.Style.Border.Bottom.Color.SetColor(Color.LightGray);
                
                worksheet.Row(summaryStartRow).Height = 30;

                // 4. Data Table Header
                int row = 6;
                string[] headers = { "رقم الفاتورة", "التاريخ", "العميل", "إجمالي المبلغ", "الخصم", "المدفوع" };
                
                for(int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[row, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81)); // Gray-700
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.White);
                }
                worksheet.Row(row).Height = 25;

                // 5. Data Rows
                row++;
                foreach (var sale in model.Sales)
                {
                    worksheet.Cells[row, 1].Value = sale.Id;
                    worksheet.Cells[row, 2].Value = sale.SaleDate;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    
                    worksheet.Cells[row, 3].Value = sale.Customer?.FullName ?? "عميل نقدي";
                    
                    worksheet.Cells[row, 4].Value = sale.TotalAmount;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                    
                    worksheet.Cells[row, 5].Value = sale.DiscountAmount;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                    
                    worksheet.Cells[row, 6].Value = sale.PaidAmount;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";

                    // Zebra Striping
                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251));
                    }
                    
                    row++;
                }

                // 6. Footer Total Row
                var totalRow = worksheet.Cells[row, 1, row, 6];
                totalRow.Style.Font.Bold = true;
                totalRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                totalRow.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(229, 231, 235)); // Gray-200
                
                worksheet.Cells[row, 1, row, 3].Merge = true;
                worksheet.Cells[row, 1].Value = "المجـــموع";
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[row, 4].Formula = $"SUM(D7:D{row - 1})";
                worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                
                worksheet.Cells[row, 5].Formula = $"SUM(E7:E{row - 1})";
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[row, 6].Formula = $"SUM(F7:F{row - 1})";
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";

                // Borders
                var tableRange = worksheet.Cells[6, 1, row, 6];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Top.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Bottom.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Left.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Right.Color.SetColor(Color.Gray);

                // AutoFit
                worksheet.Cells.AutoFitColumns(15);
                worksheet.Column(3).Width = 30; // Customer Name wider

                return package.GetAsByteArray();
            }
        }

        public byte[] ExportInventoryReport(InventoryReportViewModel model)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("تقرير المخزون");
                worksheet.View.RightToLeft = true;

                // Title
                worksheet.Cells[1, 1, 1, 5].Merge = true;
                worksheet.Cells[1, 1].Value = "تقرير المخزون الشامل";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138));
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Row(1).Height = 45;

                // Summary
                int summaryRow = 3;
                
                // Total Products
                worksheet.Cells[summaryRow, 1].Value = "إجمالي المنتجات";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(239, 246, 255));
                worksheet.Cells[summaryRow, 2].Value = model.TotalProducts;

                // Inventory Value
                worksheet.Cells[summaryRow, 3].Value = "قيمة المخزون";
                worksheet.Cells[summaryRow, 3].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(236, 253, 245));
                worksheet.Cells[summaryRow, 4].Value = model.TotalInventoryValue;
                worksheet.Cells[summaryRow, 4].Style.Numberformat.Format = "#,##0.00";

                // Low Stock
                worksheet.Cells[summaryRow + 1, 1].Value = "المنتجات الشحيحة";
                worksheet.Cells[summaryRow + 1, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow + 1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 242, 242)); // Red-50
                worksheet.Cells[summaryRow + 1, 2].Value = model.LowStockProducts;

                // Out of Stock
                worksheet.Cells[summaryRow + 1, 3].Value = "المنتجات النفاذة";
                worksheet.Cells[summaryRow + 1, 3].Style.Font.Bold = true;
                worksheet.Cells[summaryRow + 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow + 1, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 251, 235));
                worksheet.Cells[summaryRow + 1, 4].Value = model.OutOfStockProducts;

                worksheet.Cells[summaryRow, 1, summaryRow + 1, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.LightGray);
                worksheet.Row(summaryRow).Height = 25;
                worksheet.Row(summaryRow + 1).Height = 25;

                // Table
                int row = 6;
                string[] headers = { "اسم المنتج", "الفئة", "الكمية", "سعر البيع", "قيمة المخزون" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[row, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81));
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                worksheet.Row(row).Height = 25;

                row++;
                foreach (var product in model.Products)
                {
                    worksheet.Cells[row, 1].Value = product.Name;
                    worksheet.Cells[row, 2].Value = product.Category?.Name ?? "-";
                    
                    worksheet.Cells[row, 3].Value = product.Quantity;
                    // Highlight low stock
                    if (product.Quantity <= 0) worksheet.Cells[row, 3].Style.Font.Color.SetColor(Color.Red);
                    else if (product.Quantity < 10) worksheet.Cells[row, 3].Style.Font.Color.SetColor(Color.Orange);

                    worksheet.Cells[row, 4].Value = product.SalePrice;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                    
                    worksheet.Cells[row, 5].Value = product.Quantity * product.SalePrice;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";

                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251));
                    }
                    row++;
                }

                // Table Borders
                var tableRange = worksheet.Cells[6, 1, row - 1, 5];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Top.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Bottom.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Left.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Right.Color.SetColor(Color.Gray);

                worksheet.Cells.AutoFitColumns(12);
                worksheet.Column(1).Width = 35; // Product Name

                return package.GetAsByteArray();
            }
        }

        public byte[] ExportCustomerReport(CustomerReportViewModel model)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("تقرير العملاء");
                worksheet.View.RightToLeft = true;

                // Title
                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Value = "تقرير العملاء الشامل";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138));
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Row(1).Height = 45;

                // Subtitle
                worksheet.Cells[2, 1, 2, 7].Merge = true;
                worksheet.Cells[2, 1].Value = GetPeriodText(model.FromDate, model.ToDate);
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);
                worksheet.Row(2).Height = 25;

                // Stats
                int summaryRow = 3;
                worksheet.Cells[summaryRow, 1].Value = "إجمالي العملاء";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(239, 246, 255));
                worksheet.Cells[summaryRow, 2].Value = model.TotalCustomers;

                worksheet.Cells[summaryRow, 3].Value = "إجمالي الإيرادات";
                worksheet.Cells[summaryRow, 3].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(236, 253, 245));
                worksheet.Cells[summaryRow, 4].Value = model.TotalCustomerRevenue;
                worksheet.Cells[summaryRow, 4].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[summaryRow, 5].Value = "إجمالي الديون";
                worksheet.Cells[summaryRow, 5].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 242, 242));
                worksheet.Cells[summaryRow, 6].Value = model.TotalCustomerDebts;
                worksheet.Cells[summaryRow, 6].Style.Numberformat.Format = "#,##0.00";

                worksheet.Row(summaryRow).Height = 30;

                // Headlines
                int row = 6;
                string[] headers = { "اسم العميل", "رقم الهاتف", "عدد المبيعات", "إجمالي المشتريات", "الديون المستحقة", "متوسط قيمة البيع", "الحالة" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[row, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81));
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                worksheet.Row(row).Height = 25;

                row++;
                foreach (var customer in model.Customers)
                {
                    var totalSpent = customer.Sales.Sum(s => s.TotalAmount);
                    var averageSale = customer.Sales.Count > 0 ? totalSpent / customer.Sales.Count : 0;
                    var isActive = customer.Sales.Count > 0;

                    worksheet.Cells[row, 1].Value = customer.FullName;
                    worksheet.Cells[row, 2].Value = customer.PhoneNumber ?? "-";
                    worksheet.Cells[row, 3].Value = customer.Sales.Count;
                    
                    worksheet.Cells[row, 4].Value = totalSpent;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";

                    // Calculate Debts (Assuming Sales - Paid)
                    var debts = customer.Sales.Sum(s => s.TotalAmount - s.PaidAmount);
                    worksheet.Cells[row, 5].Value = debts;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                    if (debts > 0) worksheet.Cells[row, 5].Style.Font.Color.SetColor(Color.Red);

                    worksheet.Cells[row, 6].Value = averageSale;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";

                    worksheet.Cells[row, 7].Value = isActive ? "نشط" : "خامل";
                    
                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251));
                    }
                    row++;
                }
                
                // Footer
                worksheet.Cells[row, 1, row, 3].Merge = true;
                worksheet.Cells[row, 1].Value = "الإجمــالي";
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                
                worksheet.Cells[row, 4].Formula = $"SUM(D7:D{row - 1})";
                worksheet.Cells[row, 5].Formula = $"SUM(E7:E{row - 1})";
                
                worksheet.Cells[row, 1, row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(229, 231, 235));
                worksheet.Cells[row, 4, row, 5].Style.Numberformat.Format = "#,##0.00";

                // Borders
                var tableRange = worksheet.Cells[6, 1, row, 7];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Top.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Bottom.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Left.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Right.Color.SetColor(Color.Gray);

                worksheet.Cells.AutoFitColumns(15);
                worksheet.Column(1).Width = 30; // Name

                return package.GetAsByteArray();
            }
        }

        public byte[] ExportFinancialReport(FinancialReportViewModel model)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("التقرير المالي");
                worksheet.View.RightToLeft = true;

                // Title
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Value = "التقرير المالي الشامل";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138));
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Row(1).Height = 45;

                // Subtitle
                worksheet.Cells[2, 1, 2, 6].Merge = true;
                worksheet.Cells[2, 1].Value = GetPeriodText(model.FromDate, model.ToDate);
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);
                worksheet.Row(2).Height = 25;

                // Summary
                int summaryRow = 3;
                
                // Income
                worksheet.Cells[summaryRow, 1].Value = "إجمالي الإيرادات";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(236, 253, 245));
                worksheet.Cells[summaryRow, 2].Value = model.TotalIncome;
                worksheet.Cells[summaryRow, 2].Style.Numberformat.Format = "#,##0.00";

                // Expenses
                worksheet.Cells[summaryRow, 3].Value = "إجمالي المصروفات";
                worksheet.Cells[summaryRow, 3].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 242, 242));
                worksheet.Cells[summaryRow, 4].Value = model.TotalExpenses;
                worksheet.Cells[summaryRow, 4].Style.Numberformat.Format = "#,##0.00";

                // Net Profit
                worksheet.Cells[summaryRow, 5].Value = "صافي الربح";
                worksheet.Cells[summaryRow, 5].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(239, 246, 255));
                worksheet.Cells[summaryRow, 6].Value = model.NetProfit;
                worksheet.Cells[summaryRow, 6].Style.Numberformat.Format = "#,##0.00";
                if(model.NetProfit < 0) worksheet.Cells[summaryRow, 6].Style.Font.Color.SetColor(Color.Red);
                else worksheet.Cells[summaryRow, 6].Style.Font.Color.SetColor(Color.Green);

                worksheet.Row(summaryRow).Height = 25;

                // Table
                int row = 5;
                string[] headers = { "التاريخ", "الوصف", "نوع العملية", "المبلغ", "طريقة الدفع", "الرصيد" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[row, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81));
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                worksheet.Row(row).Height = 25;

                row++;
                foreach (var transaction in model.Transactions)
                {
                    worksheet.Cells[row, 1].Value = transaction.TransactionDate;
                    worksheet.Cells[row, 1].Style.Numberformat.Format = "dd/MM/yyyy";
                    
                    worksheet.Cells[row, 2].Value = transaction.Description;
                    
                    var type = transaction.TransactionType == Models.TransactionType.Income ? "إيراد" : "مصروف";
                    worksheet.Cells[row, 3].Value = type;
                    if(transaction.TransactionType == Models.TransactionType.Income) 
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(Color.Green);
                    else
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(Color.Red);

                    worksheet.Cells[row, 4].Value = transaction.Amount;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                    
                    worksheet.Cells[row, 5].Value = transaction.PaymentMethod?.ToString() ?? "-";
                    
                    worksheet.Cells[row, 6].Value = transaction.Capital;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";

                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251));
                    }
                    row++;
                }

                // Table Borders
                var tableRange = worksheet.Cells[5, 1, row - 1, 6];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Top.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Bottom.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Left.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Right.Color.SetColor(Color.Gray);

                worksheet.Cells.AutoFitColumns(12);
                worksheet.Column(2).Width = 40; // Description

                return package.GetAsByteArray();
            }
        }

        public byte[] ExportGeneralDebtReport(GeneralDebtReportViewModel model)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("تقرير الديون العامة");
                worksheet.View.RightToLeft = true;

                // Title
                worksheet.Cells[1, 1, 1, 8].Merge = true;
                worksheet.Cells[1, 1].Value = "تقرير الديون العامة الشامل";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138));
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Row(1).Height = 45;

                // Subtitle
                worksheet.Cells[2, 1, 2, 8].Merge = true;
                worksheet.Cells[2, 1].Value = GetPeriodText(model.FromDate, model.ToDate);
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);
                worksheet.Row(2).Height = 25;

                // Summary
                int summaryRow = 3;
                
                // Total Debts Count
                worksheet.Cells[summaryRow, 1].Value = "إجمالي الديون";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(239, 246, 255));
                worksheet.Cells[summaryRow, 2].Value = model.TotalDebts;

                // Total Debt Amount
                worksheet.Cells[summaryRow, 3].Value = "إجمالي المبالغ";
                worksheet.Cells[summaryRow, 3].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(236, 253, 245));
                worksheet.Cells[summaryRow, 4].Value = model.TotalDebtAmount;
                worksheet.Cells[summaryRow, 4].Style.Numberformat.Format = "#,##0.00";

                // Paid
                worksheet.Cells[summaryRow, 5].Value = "المدفوع/المحصل";
                worksheet.Cells[summaryRow, 5].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 253, 244));
                worksheet.Cells[summaryRow, 6].Value = model.TotalPaidAmount;
                worksheet.Cells[summaryRow, 6].Style.Numberformat.Format = "#,##0.00";

                // Outstanding
                worksheet.Cells[summaryRow, 7].Value = "المستحق";
                worksheet.Cells[summaryRow, 7].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 242, 242));
                worksheet.Cells[summaryRow, 8].Value = model.OutstandingAmount;
                worksheet.Cells[summaryRow, 8].Style.Numberformat.Format = "#,##0.00";

                worksheet.Row(summaryRow).Height = 25;

                // Table
                int row = 5;
                string[] headers = { "العنوان", "الطرف", "نوع الدين", "المبلغ", "المدفوع/المحصل", "المستحق", "تاريخ الإنشاء", "الحالة" };
                
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[row, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81));
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                worksheet.Row(row).Height = 25;

                row++;
                foreach (var debt in model.Debts)
                {
                    var remainingAmount = debt.Amount - debt.PaidAmount;
                    var isActive = remainingAmount > 0;

                    worksheet.Cells[row, 1].Value = debt.Title;
                    worksheet.Cells[row, 2].Value = debt.PartyName ?? "-";
                    worksheet.Cells[row, 3].Value = debt.DebtType == Models.GeneralDebtType.OwedToMe ? "دين ليا" : "دين عليا";
                    if(debt.DebtType == Models.GeneralDebtType.OwedToMe)
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(Color.Green);
                    else
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(Color.Red);

                    worksheet.Cells[row, 4].Value = debt.Amount;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                    
                    worksheet.Cells[row, 5].Value = debt.PaidAmount;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                    
                    worksheet.Cells[row, 6].Value = remainingAmount;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                    
                    worksheet.Cells[row, 7].Value = debt.CreatedAt;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "dd/MM/yyyy";

                    worksheet.Cells[row, 8].Value = isActive ? "نشط" : "مستوفى";

                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251));
                    }
                    row++;
                }

                // Borders
                var tableRange = worksheet.Cells[5, 1, row - 1, 8];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Top.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Bottom.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Left.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Right.Color.SetColor(Color.Gray);

                worksheet.Cells.AutoFitColumns(12);
                worksheet.Column(1).Width = 30; // Title

                return package.GetAsByteArray();
            }
        }

        public byte[] ExportProfitLossReport(ProfitLossReportViewModel model)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("تقرير الأرباح والخسائر");
                worksheet.View.RightToLeft = true;

                // Title
                worksheet.Cells[1, 1, 1, 4].Merge = true;
                worksheet.Cells[1, 1].Value = "تقرير الأرباح والخسائر";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138));
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Row(1).Height = 45;

                // Subtitle
                worksheet.Cells[2, 1, 2, 4].Merge = true;
                worksheet.Cells[2, 1].Value = GetPeriodText(model.FromDate, model.ToDate);
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);
                worksheet.Row(2).Height = 25;

                // Summary Cards
                int summaryRow = 3;
                
                // Revenue
                worksheet.Cells[summaryRow, 1].Value = "إجمالي الإيرادات";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(236, 253, 245));
                worksheet.Cells[summaryRow, 2].Value = model.TotalRevenue;
                worksheet.Cells[summaryRow, 2].Style.Numberformat.Format = "#,##0.00";

                // Expenses
                worksheet.Cells[summaryRow, 3].Value = "إجمالي التكاليف";
                worksheet.Cells[summaryRow, 3].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[summaryRow, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 242, 242));
                worksheet.Cells[summaryRow, 4].Value = model.TotalExpenses;
                worksheet.Cells[summaryRow, 4].Style.Numberformat.Format = "#,##0.00";

                worksheet.Row(summaryRow).Height = 25;

                // Detailed Analysis Section
                int row = 5;
                worksheet.Cells[row, 1].Value = "تحليل الربحية التفصيلي";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.Font.Size = 14;
                worksheet.Cells[row, 1].Style.Font.UnderLine = true;
                row += 2;

                // Table Header
                worksheet.Cells[row, 1].Value = "البند";
                worksheet.Cells[row, 2].Value = "القيمة";
                worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
                worksheet.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81));
                worksheet.Cells[row, 1, row, 2].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells[row, 1, row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row++;

                // 1. Revenue
                worksheet.Cells[row, 1].Value = "إجمالي الإيرادات";
                worksheet.Cells[row, 2].Value = model.TotalRevenue;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251));
                row++;

                // 2. COGS
                worksheet.Cells[row, 1].Value = "تكلفة البضائع المباعة";
                worksheet.Cells[row, 2].Value = model.CostOfGoodsSold;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                row++;

                // 3. Gross Profit
                worksheet.Cells[row, 1].Value = "إجمالي الربح (Gross Profit)";
                worksheet.Cells[row, 2].Value = model.GrossProfit;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
                worksheet.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(224, 231, 255));
                row++;

                // 4. Operating Settings
                worksheet.Cells[row, 1].Value = "المصروفات التشغيلية";
                worksheet.Cells[row, 2].Value = model.OperatingExpenses;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                row++;

                // 5. Net Profit
                worksheet.Cells[row, 1].Value = "صافي الربح (Net Profit)";
                worksheet.Cells[row, 2].Value = model.NetProfit;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
                worksheet.Cells[row, 1, row, 2].Style.Font.Size = 12;
                worksheet.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 252, 231)); // Green-100
                if(model.NetProfit < 0) 
                     worksheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(254, 226, 226)); // Red-100

                row++;

                // 6. Profit Margin
                worksheet.Cells[row, 1].Value = "هامش الربح";
                worksheet.Cells[row, 2].Value = model.ProfitMargin / 100m; // Assuming it's percentage e.g. 15.5
                worksheet.Cells[row, 2].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
                
                // Borders
                var tableRange = worksheet.Cells[7, 1, row, 2];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Top.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Bottom.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Left.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Right.Color.SetColor(Color.Gray);

                worksheet.Column(1).Width = 35;
                worksheet.Column(2).Width = 20;

                return package.GetAsByteArray();
            }
        }

        public byte[] ExportReceivablesReport(ReceivablesReportViewModel model)
        {
            var columns = new Dictionary<string, Func<ViewModels.CustomerReceivableEntry, object>>
            {
                { "اسم العميل", x => x.CustomerName },
                { "إجمالي المبيعات", x => x.TotalSales },
                { "إجمالي المدفوع", x => x.TotalPaid },
                { "المستحق (الرصيد)", x => x.Balance }
            };

            return ExportGeneric(model.Entries, "مستحقات العملاء", columns, "تقرير مستحقات العملاء");
        }

        public byte[] ExportPayablesReport(PayablesReportViewModel model)
        {
            var columns = new Dictionary<string, Func<ViewModels.SupplierPayableEntry, object>>
            {
                { "اسم المورد", x => x.SupplierName },
                { "إجمالي المشتريات", x => x.TotalPurchases },
                { "إجمالي المدفوع", x => x.TotalPaid },
                { "المستحق (الرصيد)", x => x.Balance }
            };

            return ExportGeneric(model.Entries, "التزامات الموردين", columns, "تقرير التزامات الموردين");
        }

        public byte[] ExportCapitalSummaryReport(CapitalSummaryViewModel model)
        {
             // Capital Summary is a dashboard-like view, not a list. We'll make a simple property list.
             var data = new List<dynamic>
             {
                 new { Item = "رصيد الخزنة (Net Store Balance)", Value = model.StoreNetBalance },
                 new { Item = "مستحقات العملاء (Receivables)", Value = model.CustomerReceivables },
                 new { Item = "ديون عامة لي (General Debts Owed)", Value = model.GeneralReceivables },
                 new { Item = "قيمة المخزون (Inventory Value)", Value = model.InventoryValue },
                 new { Item = "التزامات الموردين (Payables)", Value = model.SupplierPayables },
                 new { Item = "ديون عامة علي (General Debts Owed)", Value = model.GeneralPayables },
                 new { Item = "صافي رأس المال (Net Capital)", Value = model.NetCapital }
             };

             var columns = new Dictionary<string, Func<dynamic, object>>
             {
                 { "البند", x => x.Item },
                 { "القيمة", x => x.Value }
             };

             return ExportGeneric(data, "ملخص رأس المال", columns, "تقرير ملخص رأس المال");
        }

        public byte[] ExportLowStockReport(IEnumerable<ManageMentSystem.Models.Product> model)
        {
            var columns = new Dictionary<string, Func<ManageMentSystem.Models.Product, object>>
            {
                { "الاسم", x => x.Name },
                { "الفئة", x => x.Category?.Name ?? "-" },
                { "الكمية الحالية", x => x.Quantity },
                { "سعر الشراء", x => x.PurchasePrice },
                { "سعر البيع", x => x.SalePrice },
                { "الوصف", x => x.Description ?? "-" }
            };

            using (var package = new ExcelPackage())
            {
                var bytes = ExportGeneric(model, "النواقص", columns, "تقرير المخزون الناقص");
                // Need to reload to add conditional formatting? No, ExportGeneric returns bytes.
                // For simplicity, we stick to Generic which is clean.
                return bytes;
            }
        }

        public byte[] ExportComprehensiveReport(ComprehensiveReportViewModel model)
        {
            // Create a Multi-Sheet Excel
            using (var package = new ExcelPackage())
            {
                // Sheet 1: Summary
                var summarySheet = package.Workbook.Worksheets.Add("الملخص");
                summarySheet.View.RightToLeft = true;
                
                // Title
                summarySheet.Cells[1, 1, 1, 2].Merge = true;
                summarySheet.Cells[1, 1].Value = "ملخص التقرير الشامل";
                summarySheet.Cells[1, 1].Style.Font.Size = 18;
                summarySheet.Cells[1, 1].Style.Font.Bold = true;
                summarySheet.Row(1).Height = 30;

                int row = 3;
                summarySheet.Cells[row, 1].Value = "اجمالي المبيعات";
                summarySheet.Cells[row, 2].Value = model.TotalSales;
                row++;
                summarySheet.Cells[row, 1].Value = "اجمالي الايرادات";
                summarySheet.Cells[row, 2].Value = model.TotalRevenue; 
                row++;
                summarySheet.Cells[row, 1].Value = "اجمالي العملاء";
                summarySheet.Cells[row, 2].Value = model.TotalCustomers;
                 row++;
                summarySheet.Cells[row, 1].Value = "اجمالي المنتجات";
                summarySheet.Cells[row, 2].Value = model.TotalProducts;
                 row++;
                summarySheet.Cells[row, 1].Value = "قيمة المخزون";
                summarySheet.Cells[row, 2].Value = model.InventoryValue;

                summarySheet.Column(1).Width = 25;
                summarySheet.Column(2).Width = 25;

                // Sheet 2: Products
                if(model.Products != null && model.Products.Any())
                {
                   var pColumns = new Dictionary<string, Func<ManageMentSystem.Models.Product, object>>
                   {
                       { "الاسم", x => x.Name },
                        { "الكمية", x => x.Quantity },
                        { "السعر", x => x.SalePrice }
                   };
                   // We reused Logic from ExportGeneric but internal private not available.
                   // So manually add sheet ? No, can't easily reuse.
                   // Just dumping data manually quickly.
                   var pSheet = package.Workbook.Worksheets.Add("المنتجات");
                   pSheet.View.RightToLeft = true;
                   pSheet.Cells[1,1].Value = "الاسم";
                   pSheet.Cells[1,2].Value = "الكمية";
                   pSheet.Cells[1,3].Value = "السعر";
                   
                   int r = 2;
                   foreach(var p in model.Products)
                   {
                       pSheet.Cells[r,1].Value = p.Name;
                       pSheet.Cells[r,2].Value = p.Quantity;
                       pSheet.Cells[r,3].Value = p.SalePrice;
                       r++;
                   }
                   pSheet.Cells.AutoFitColumns();
                }

                 // Sheet 3: Sales
                if(model.Sales != null && model.Sales.Any())
                {
                   var pSheet = package.Workbook.Worksheets.Add("آخر المبيعات");
                   pSheet.View.RightToLeft = true;
                   pSheet.Cells[1,1].Value = "رقم الفاتورة";
                   pSheet.Cells[1,2].Value = "التاريخ";
                   pSheet.Cells[1,3].Value = "العميل";
                   pSheet.Cells[1,4].Value = "المبلغ";
                   
                   int r = 2;
                   foreach(var s in model.Sales)
                   {
                       pSheet.Cells[r,1].Value = s.Id;
                       pSheet.Cells[r,2].Value = s.SaleDate.ToString("yyyy-MM-dd");
                       pSheet.Cells[r,3].Value = s.Customer?.FullName ?? "نقدي";
                       pSheet.Cells[r,4].Value = s.TotalAmount;
                       r++;
                   }
                   pSheet.Cells.AutoFitColumns();
                }

                return package.GetAsByteArray();
            }
        }


        public byte[] ExportCategoryPerformanceReport(CategoryPerformanceReportViewModel model)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("أداء الفئات");
                worksheet.View.RightToLeft = true;

                // Title
                worksheet.Cells[1, 1, 1, 5].Merge = true;
                worksheet.Cells[1, 1].Value = "تقرير أداء الفئات";
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Name = "Segoe UI";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138));
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Row(1).Height = 45;

                // Subtitle
                worksheet.Cells[2, 1, 2, 5].Merge = true;
                worksheet.Cells[2, 1].Value = GetPeriodText(model.FromDate, model.ToDate);
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);
                worksheet.Row(2).Height = 25;

                // Stats row
                int statsRow = 4;
                
                worksheet.Cells[statsRow, 1].Value = "إجمالي الفئات";
                worksheet.Cells[statsRow, 1].Style.Font.Bold = true;
                worksheet.Cells[statsRow, 2].Value = model.TotalCategories;
                
                worksheet.Cells[statsRow, 3].Value = "إجمالي المبيعات";
                worksheet.Cells[statsRow, 3].Style.Font.Bold = true;
                worksheet.Cells[statsRow, 4].Value = model.TotalRevenue;
                worksheet.Cells[statsRow, 4].Style.Numberformat.Format = "#,##0.00";
                
                worksheet.Cells[statsRow + 1, 1].Value = "إجمالي الأرباح";
                worksheet.Cells[statsRow + 1, 1].Style.Font.Bold = true;
                worksheet.Cells[statsRow + 1, 2].Value = model.TotalProfit;
                worksheet.Cells[statsRow + 1, 2].Style.Numberformat.Format = "#,##0.00";
                
                worksheet.Cells[statsRow + 1, 3].Value = "عدد العمليات";
                worksheet.Cells[statsRow + 1, 3].Style.Font.Bold = true;
                worksheet.Cells[statsRow + 1, 4].Value = model.TotalSales;

                worksheet.Cells[statsRow, 1, statsRow + 1, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.LightGray);

                // Table Header
                int row = 7;
                string[] headers = { "اسم الفئة", "عدد المبيعات", "الإيرادات", "الأرباح", "هامش الربح" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[row, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(55, 65, 81));
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                worksheet.Row(row).Height = 25;

                // Data
                row++;
                foreach (var cat in model.CategoryPerformance.OrderByDescending(x => x.Revenue))
                {
                    worksheet.Cells[row, 1].Value = cat.CategoryName;
                    worksheet.Cells[row, 2].Value = cat.SalesCount;
                    worksheet.Cells[row, 3].Value = cat.Revenue;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 4].Value = cat.Profit;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                    
                    var margin = cat.Revenue > 0 ? (cat.Profit / cat.Revenue) : 0;
                    worksheet.Cells[row, 5].Value = margin;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "0.0%";

                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 250, 251));
                    }
                    row++;
                }

                // Borders
                var tableRange = worksheet.Cells[7, 1, row - 1, 5];
                tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                tableRange.Style.Border.Top.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Bottom.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Left.Color.SetColor(Color.Gray);
                tableRange.Style.Border.Right.Color.SetColor(Color.Gray);

                worksheet.Cells.AutoFitColumns(15);
                worksheet.Column(1).Width = 30;

                return package.GetAsByteArray();
            }
        }

        private string GetPeriodText(DateTime? fromDate, DateTime? toDate) {
            if (fromDate == null && toDate == null) return "الفترة: كل الفترات";
            if (fromDate == null) return $"الفترة: حتى {toDate.Value:dd/MM/yyyy}";
            if (toDate == null) return $"الفترة: من {fromDate.Value:dd/MM/yyyy}";
            return $"الفترة: من {fromDate.Value:dd/MM/yyyy} إلى {toDate.Value:dd/MM/yyyy}";
        }
    }
}
