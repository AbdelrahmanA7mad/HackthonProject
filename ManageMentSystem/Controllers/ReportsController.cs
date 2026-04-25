using ManageMentSystem.Data;
using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services;
using ManageMentSystem.Services.ExcelExportServices;


using ManageMentSystem.Services.CustomerServices;
using ManageMentSystem.Services.ProductServices;
using ManageMentSystem.Services.SalesServices;
using ManageMentSystem.Services.StatisticsServices;
using ManageMentSystem.Services.SystemSettings;
using ManageMentSystem.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        private readonly ICustomerService _customerService;
        private readonly AppDbContext _context;
        private readonly IExcelExportService _excelExportService;
        private readonly IUserService _userService;

        public ReportsController(
            IStatisticsService statisticsService,
            IProductService productService,
            ISalesService salesService,
            ICustomerService customerService,
            AppDbContext context,
            IExcelExportService excelExportService,
            IUserService userService)
        {
            _statisticsService = statisticsService;
            _productService = productService;
            _salesService = salesService;
            _customerService = customerService;
            _context = context;
            _excelExportService = excelExportService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _statisticsService.GetStatisticsAsync();
            return View(stats);
        }

        public async Task<IActionResult> SalesStats(string period = "today")
        {
            var stats = await _statisticsService.GetFilteredStatisticsAsync(period, null, null, null);
            ViewBag.Period = period;
            return View(stats);
        }

        public async Task<IActionResult> Comprehensive(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetComprehensiveReportAsync(fromDate, toDate);
            return View(report);
        }

        public async Task<IActionResult> LowStock(int? categoryId = null)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            var systemSettingsService = HttpContext.RequestServices.GetService<ISystemSettingsService>();
            var inventorySettings = await systemSettingsService.GetInventorySettingsAsync();

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity <= inventorySettings.LowStockThreshold && p.TenantId == tenantId);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            var products = await query.ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.IsActive && c.TenantId == tenantId)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.LowStockThreshold = inventorySettings.LowStockThreshold;

            return View(products);
        }

        public async Task<IActionResult> SalesReport(DateTime? fromDate, DateTime? toDate, int? customerId, int? categoryId)
        {
            var report = await _statisticsService.GetSalesReportAsync(fromDate, toDate, customerId, categoryId);
            return View(report);
        }

        public async Task<IActionResult> InventoryReport(int? categoryId, bool? lowStockOnly)
        {
            var report = await _statisticsService.GetInventoryReportAsync(categoryId, lowStockOnly);
            return View(report);
        }

        public async Task<IActionResult> CustomerReport(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetCustomerReportAsync(fromDate, toDate);
            return View(report);
        }

        public async Task<IActionResult> FinancialReport(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetFinancialReportAsync(fromDate, toDate);
            return View(report);
        }

        
        public async Task<IActionResult> GeneralDebtReport(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetGeneralDebtReportAsync(fromDate, toDate);
            return View(report);
        }

        
        public async Task<IActionResult> StoreAccountReport(DateTime? fromDate, DateTime? toDate, string? transactionType)
        {
            var report = await _statisticsService.GetStoreAccountReportAsync(fromDate, toDate, transactionType);
            return View(report);
        }
        
        public async Task<IActionResult> ProfitLossReport(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetProfitLossReportAsync(fromDate, toDate);
            return View(report);
        }

        
        public async Task<IActionResult> CategoryPerformanceReport(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetCategoryPerformanceReportAsync(fromDate, toDate);
            return View(report);
        }

        // Excel Export Actions
        
        public async Task<IActionResult> ExportSalesReportExcel(DateTime? fromDate, DateTime? toDate, int? customerId, int? categoryId)
        {
            var report = await _statisticsService.GetSalesReportAsync(fromDate, toDate, customerId, categoryId);
            var fileContent = _excelExportService.ExportSalesReport(report);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        public async Task<IActionResult> ExportInventoryReportExcel(int? categoryId, bool? lowStockOnly)
        {
            var report = await _statisticsService.GetInventoryReportAsync(categoryId, lowStockOnly);
            var fileContent = _excelExportService.ExportInventoryReport(report);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"InventoryReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        public async Task<IActionResult> ExportCustomerReportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetCustomerReportAsync(fromDate, toDate);
            var fileContent = _excelExportService.ExportCustomerReport(report);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CustomerReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        public async Task<IActionResult> ExportFinancialReportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetFinancialReportAsync(fromDate, toDate);
            var fileContent = _excelExportService.ExportFinancialReport(report);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"FinancialReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        
        public async Task<IActionResult> ExportGeneralDebtReportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetGeneralDebtReportAsync(fromDate, toDate);
            var fileContent = _excelExportService.ExportGeneralDebtReport(report);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"GeneralDebtReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        public async Task<IActionResult> ExportProfitLossReportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var report = await _statisticsService.GetProfitLossReportAsync(fromDate, toDate);
            var fileContent = _excelExportService.ExportProfitLossReport(report);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ProfitLossReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }


        
        public async Task<IActionResult> ExportReceivablesReportExcel()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            var customers = await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .Select(c => new
                {
                    c.Id,
                    c.FullName,
                    TotalSales = _context.Sales.Where(s => s.CustomerId == c.Id && s.TenantId == tenantId).Sum(s => (decimal?)s.TotalAmount) ?? 0m,
                    TotalPaid = _context.Sales.Where(s => s.CustomerId == c.Id && s.TenantId == tenantId).Sum(s => (decimal?)s.PaidAmount) ?? 0m
                })
                .OrderByDescending(x => (x.TotalSales - x.TotalPaid))
                .ToListAsync();

            var vm = new ViewModels.ReceivablesReportViewModel
            {
                Entries = customers.Select(x => new ViewModels.CustomerReceivableEntry
                {
                    CustomerId = x.Id,
                    CustomerName = x.FullName,
                    TotalSales = x.TotalSales,
                    TotalPaid = x.TotalPaid
                }).Where(e => e.Balance > 0).ToList()
            };
            vm.TotalReceivables = vm.Entries.Sum(e => e.Balance);

            var fileContent = _excelExportService.ExportReceivablesReport(vm);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ReceivablesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        
        public async Task<IActionResult> ExportCapitalSummaryReportExcel()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            var storeIncome = await _context.StoreAccounts
                .Where(x => x.TransactionType == Models.TransactionType.Income && x.TenantId == tenantId)
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            var storeExpense = await _context.StoreAccounts
                .Where(x => x.TransactionType == Models.TransactionType.Expense && x.TenantId == tenantId)
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            var storeNet = storeIncome - storeExpense;

            var customerReceivables = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .SumAsync(s => (decimal?)(s.TotalAmount - s.PaidAmount)) ?? 0m;

            var generalReceivables = await _context.GeneralDebts
                .Where(d => d.DebtType == ManageMentSystem.Models.GeneralDebtType.OwedToMe && d.TenantId == tenantId)
                .SumAsync(d => (decimal?)(d.Amount - d.PaidAmount)) ?? 0m;

            var generalPayables = await _context.GeneralDebts
                .Where(d => d.DebtType == ManageMentSystem.Models.GeneralDebtType.OnMe && d.TenantId == tenantId)
                .SumAsync(d => (decimal?)(d.Amount - d.PaidAmount)) ?? 0m;

      

            var inventoryValue = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => (decimal?)(p.Quantity * p.PurchasePrice)) ?? 0m;

            var vm = new ViewModels.CapitalSummaryViewModel
            {
                StoreNetBalance = storeNet,
                CustomerReceivables = Math.Max(0, customerReceivables),
                GeneralReceivables = Math.Max(0, generalReceivables),
                GeneralPayables = Math.Max(0, generalPayables),
                InventoryValue = Math.Max(0, inventoryValue)
            };

            var fileContent = _excelExportService.ExportCapitalSummaryReport(vm);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CapitalSummary_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        public async Task<IActionResult> ExportLowStockReportExcel(int? categoryId = null)
        {
            // Similar logic to LowStock action
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            var systemSettingsService = HttpContext.RequestServices.GetService<ISystemSettingsService>();
            var inventorySettings = await systemSettingsService.GetInventorySettingsAsync();

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity <= inventorySettings.LowStockThreshold && p.TenantId == tenantId);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            var products = await query.ToListAsync();

            var fileContent = _excelExportService.ExportLowStockReport(products);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"LowStockReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        
        public async Task<IActionResult> ExportComprehensiveReportExcel(DateTime? fromDate, DateTime? toDate)
        {
             var report = await _statisticsService.GetComprehensiveReportAsync(fromDate, toDate);
             var fileContent = _excelExportService.ExportComprehensiveReport(report);
             return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ComprehensiveReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }



        // حسابات العملاء
        
        public async Task<IActionResult> Receivables()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            var customers = await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .Select(c => new
                {
                    c.Id,
                    c.FullName,
                    TotalSales = _context.Sales.Where(s => s.CustomerId == c.Id && s.TenantId == tenantId).Sum(s => (decimal?)s.TotalAmount) ?? 0m,
                    TotalPaid = _context.Sales.Where(s => s.CustomerId == c.Id && s.TenantId == tenantId).Sum(s => (decimal?)s.PaidAmount) ?? 0m
                })
                .OrderByDescending(x => (x.TotalSales - x.TotalPaid))
                .ToListAsync();

            var vm = new ViewModels.ReceivablesReportViewModel
            {
                Entries = customers.Select(x => new ViewModels.CustomerReceivableEntry
                {
                    CustomerId = x.Id,
                    CustomerName = x.FullName,
                    TotalSales = x.TotalSales,
                    TotalPaid = x.TotalPaid
                }).Where(e => e.Balance > 0).ToList()
            };

            vm.TotalReceivables = vm.Entries.Sum(e => e.Balance);
            return View(vm);
        }

        // حسابات الموردين
        
      

        // ملخص رأس المال
        
        public async Task<IActionResult> CapitalSummary()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            var storeIncome = await _context.StoreAccounts
                .Where(x => x.TransactionType == Models.TransactionType.Income && x.TenantId == tenantId)
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            var storeExpense = await _context.StoreAccounts
                .Where(x => x.TransactionType == Models.TransactionType.Expense && x.TenantId == tenantId)
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            var storeNet = storeIncome - storeExpense;

            var customerReceivables = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .SumAsync(s => (decimal?)(s.TotalAmount - s.PaidAmount)) ?? 0m;

            var generalReceivables = await _context.GeneralDebts
                .Where(d => d.DebtType == ManageMentSystem.Models.GeneralDebtType.OwedToMe && d.TenantId == tenantId)
                .SumAsync(d => (decimal?)(d.Amount - d.PaidAmount)) ?? 0m;

            var generalPayables = await _context.GeneralDebts
                .Where(d => d.DebtType == ManageMentSystem.Models.GeneralDebtType.OnMe && d.TenantId == tenantId)
                .SumAsync(d => (decimal?)(d.Amount - d.PaidAmount)) ?? 0m;

        
            var inventoryValue = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => (decimal?)(p.Quantity * p.PurchasePrice)) ?? 0m;

            var vm = new ViewModels.CapitalSummaryViewModel
            {
                StoreNetBalance = storeNet,
                CustomerReceivables = Math.Max(0, customerReceivables),
                GeneralReceivables = Math.Max(0, generalReceivables),
                GeneralPayables = Math.Max(0, generalPayables),
                InventoryValue = Math.Max(0, inventoryValue)
            };

            return View(vm);
        }
    }
}
