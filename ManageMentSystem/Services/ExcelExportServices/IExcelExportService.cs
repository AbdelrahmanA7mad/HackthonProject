using ManageMentSystem.ViewModels;
using System.Collections.Generic;
using System;

namespace ManageMentSystem.Services.ExcelExportServices
{
    public interface IExcelExportService
    {
        byte[] ExportSalesReport(SalesReportViewModel model);
        byte[] ExportInventoryReport(InventoryReportViewModel model);
        byte[] ExportCustomerReport(CustomerReportViewModel model);
        byte[] ExportFinancialReport(FinancialReportViewModel model);
        byte[] ExportGeneralDebtReport(GeneralDebtReportViewModel model);
        byte[] ExportProfitLossReport(ProfitLossReportViewModel model);
        byte[] ExportReceivablesReport(ReceivablesReportViewModel model);
        byte[] ExportPayablesReport(PayablesReportViewModel model);
        byte[] ExportCapitalSummaryReport(CapitalSummaryViewModel model);
        byte[] ExportLowStockReport(IEnumerable<ManageMentSystem.Models.Product> model);
        byte[] ExportComprehensiveReport(ComprehensiveReportViewModel model);
        byte[] ExportGeneric<T>(IEnumerable<T> data, string sheetName, Dictionary<string, Func<T, object>> columns, string title = "");
    }
}
