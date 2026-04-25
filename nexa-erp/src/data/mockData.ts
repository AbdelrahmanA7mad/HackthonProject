export const initialProducts = [
  { id: 1, name: 'لابتوب ديل XPS', category: 'إلكترونيات', quantity: 15, purchasePrice: 40000, salePrice: 45000, barcode: '123456789' },
  { id: 2, name: 'شاشة سامسونج 27 بوصة', category: 'إلكترونيات', quantity: 30, purchasePrice: 5000, salePrice: 6500, barcode: '987654321' },
  { id: 3, name: 'طابعة إتش بي', category: 'معدات مكتبية', quantity: 8, purchasePrice: 3000, salePrice: 4000, barcode: '456123789' },
  { id: 4, name: 'ماوس لاسلكي لوجيتك', category: 'إكسسوارات', quantity: 50, purchasePrice: 500, salePrice: 800, barcode: '789123456' },
  { id: 5, name: 'لوحة مفاتيح ميكانيكية', category: 'إكسسوارات', quantity: 20, purchasePrice: 1500, salePrice: 2200, barcode: '321654987' },
];

export const initialCustomers = [
  { id: 1, name: 'أحمد محمود', phone: '01012345678', address: 'القاهرة، مدينة نصر', salesCount: 5 },
  { id: 2, name: 'محمد علي', phone: '01198765432', address: 'الجيزة، الدقي', salesCount: 2 },
  { id: 3, name: 'سارة إبراهيم', phone: '01234567890', address: 'الإسكندرية، سموحة', salesCount: 8 },
  { id: 4, name: 'محمود حسن', phone: '01511223344', address: 'القاهرة، المعادي', salesCount: 0 },
];

export const initialSales = [
  { id: 101, customerId: 1, date: '2026-04-25', totalAmount: 45000, paymentType: 'كاش', status: 'خالص' },
  { id: 102, customerId: 3, date: '2026-04-24', totalAmount: 13000, paymentType: 'آجل', status: 'غير مسدد' },
  { id: 103, customerId: 2, date: '2026-04-20', totalAmount: 4000, paymentType: 'جزئي', status: 'جزئي' },
  { id: 104, customerId: 1, date: '2026-04-15', totalAmount: 6500, paymentType: 'كاش', status: 'خالص' },
  { id: 105, customerId: 3, date: '2026-04-10', totalAmount: 800, paymentType: 'كاش', status: 'خالص' },
];

export const monthlyRevenueData = [
  // Historical Data
  { name: 'يناير', revenue: 120000, expenses: 80000, isForecast: false },
  { name: 'فبراير', revenue: 150000, expenses: 90000, isForecast: false },
  { name: 'مارس', revenue: 180000, expenses: 95000, isForecast: false },
  { name: 'أبريل', revenue: 220000, expenses: 100000, isForecast: false },
  
  // Forecast Data
  { name: 'مايو (توقع)', forecastRevenue: 255000, forecastExpenses: 110000, isForecast: true },
  { name: 'يونيو (توقع)', forecastRevenue: 290000, forecastExpenses: 115000, isForecast: true },
];

export const initialCategories = [
  { id: 1, name: 'إلكترونيات', description: 'أجهزة كمبيوتر ولابتوب وشاشات', status: 'نشط' },
  { id: 2, name: 'معدات مكتبية', description: 'طابعات وآلات تصوير', status: 'نشط' },
  { id: 3, name: 'إكسسوارات', description: 'ماوس، كيبورد، كابلات', status: 'نشط' },
];

export const initialDebts = [
  { id: 1, name: 'شركة التوريدات الحديثة', amount: 15000, date: '2026-04-01', notes: 'دفعة أجهزة كمبيوتر', status: 'غير مسدد' },
  { id: 2, name: 'مصاريف إيجار', amount: 5000, date: '2026-04-15', notes: 'إيجار شهر أبريل', status: 'مسدد' },
];

export const initialInstallments = [
  { id: 1, customerId: 3, totalAmount: 13000, paidAmount: 3000, remainingAmount: 10000, monthlyPayment: 2000, nextDueDate: '2026-05-24' },
];

export const storeAccountSummary = {
  currentBalance: 85000,
  totalIncome: 350000,
  totalExpenses: 265000
};
