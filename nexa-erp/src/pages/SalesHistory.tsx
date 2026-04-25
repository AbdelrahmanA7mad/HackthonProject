import React, { useState } from 'react';
import { Search, ShoppingCart, Eye, FileText, Printer } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const SalesHistory = () => {
  const { state } = useAppContext();
  const [searchTerm, setSearchTerm] = useState('');
  const [filter, setFilter] = useState('الكل');

  const filteredSales = state.sales.filter(sale => {
    const matchesSearch = sale.id.toString().includes(searchTerm);
    const matchesFilter = filter === 'الكل' || sale.status === filter;
    return matchesSearch && matchesFilter;
  });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <ShoppingCart className="text-[#0f1419]" /> سجل المبيعات
          </h1>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100 bg-gray-50 flex flex-col md:flex-row gap-4 justify-between items-center">
          <div className="relative w-full md:w-96">
            <Search className="absolute right-3 top-2.5 text-gray-400" size={18} />
            <input 
              type="text" 
              placeholder="ابحث برقم الفاتورة..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full bg-white border border-gray-200 focus:border-[#0f1419] rounded-xl py-2 pr-10 pl-4 outline-none transition-all text-sm"
            />
          </div>
          <div className="flex gap-2 w-full md:w-auto overflow-x-auto pb-1 no-scrollbar">
            {['الكل', 'خالص', 'جزئي', 'غير مسدد'].map(f => (
              <button 
                key={f}
                onClick={() => setFilter(f)}
                className={`px-4 py-2 rounded-xl text-sm font-bold whitespace-nowrap transition-colors ${filter === f ? 'bg-[#0f1419] text-white' : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'}`}
              >
                {f}
              </button>
            ))}
          </div>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full text-right">
            <thead className="bg-gray-50 text-gray-500 text-sm border-b border-gray-100">
              <tr>
                <th className="px-6 py-4 font-bold">رقم الفاتورة</th>
                <th className="px-6 py-4 font-bold">التاريخ</th>
                <th className="px-6 py-4 font-bold">العميل</th>
                <th className="px-6 py-4 font-bold">المبلغ الإجمالي</th>
                <th className="px-6 py-4 font-bold">طريقة الدفع</th>
                <th className="px-6 py-4 font-bold">الحالة</th>
                <th className="px-6 py-4 font-bold text-center">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50 text-sm">
              {filteredSales.map(sale => {
                const customer = state.customers.find(c => c.id === sale.customerId);
                return (
                  <tr key={sale.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 font-bold text-[#0f1419]">#{sale.id}</td>
                    <td className="px-6 py-4 text-gray-500">{sale.date}</td>
                    <td className="px-6 py-4 font-bold text-[#0f1419]">{customer ? customer.name : 'عميل نقدي'}</td>
                    <td className="px-6 py-4 font-bold text-[#0f1419]">{sale.totalAmount.toLocaleString()} ج.م</td>
                    <td className="px-6 py-4 text-gray-500">{sale.paymentType}</td>
                    <td className="px-6 py-4">
                      <span className={`px-3 py-1 rounded-full font-bold text-xs ${
                        sale.status === 'خالص' ? 'bg-emerald-50 text-emerald-700' : 
                        sale.status === 'جزئي' ? 'bg-amber-50 text-amber-700' : 'bg-red-50 text-red-700'
                      }`}>
                        {sale.status}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-center gap-2">
                        <button className="w-8 h-8 rounded-lg bg-gray-50 text-[#0f1419] flex items-center justify-center hover:bg-gray-200 transition-colors">
                          <Eye size={14} />
                        </button>
                        <button className="w-8 h-8 rounded-lg bg-gray-50 text-[#0f1419] flex items-center justify-center hover:bg-gray-200 transition-colors">
                          <Printer size={14} />
                        </button>
                        <button className="w-8 h-8 rounded-lg bg-gray-50 text-[#0f1419] flex items-center justify-center hover:bg-gray-200 transition-colors">
                          <FileText size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
