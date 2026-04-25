import React, { useState } from 'react';
import { CalendarDays, Search, CreditCard, AlertCircle } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const Installments = () => {
  const { state } = useAppContext();
  const [searchTerm, setSearchTerm] = useState('');

  const filteredInstallments = state.installments.filter(inst => {
    const customer = state.customers.find(c => c.id === inst.customerId);
    return customer?.name.includes(searchTerm) || inst.id.toString().includes(searchTerm);
  });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <CalendarDays className="text-[#0f1419]" /> إدارة الأقساط
          </h1>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100 bg-gray-50 flex justify-between items-center">
          <div className="relative w-full md:w-96">
            <Search className="absolute right-3 top-2.5 text-gray-400" size={18} />
            <input 
              type="text" 
              placeholder="ابحث باسم العميل أو رقم القسط..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full bg-white border border-gray-200 focus:border-[#0f1419] rounded-xl py-2 pr-10 pl-4 outline-none transition-all text-sm"
            />
          </div>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full text-right">
            <thead className="bg-gray-50 text-gray-500 text-sm border-b border-gray-100">
              <tr>
                <th className="px-6 py-4 font-bold">رقم القسط</th>
                <th className="px-6 py-4 font-bold">العميل</th>
                <th className="px-6 py-4 font-bold">إجمالي المبلغ</th>
                <th className="px-6 py-4 font-bold">المدفوع</th>
                <th className="px-6 py-4 font-bold">المتبقي</th>
                <th className="px-6 py-4 font-bold">تاريخ الدفعة القادمة</th>
                <th className="px-6 py-4 font-bold text-center">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50 text-sm">
              {filteredInstallments.map(inst => {
                const customer = state.customers.find(c => c.id === inst.customerId);
                const isLate = new Date(inst.nextDueDate) < new Date();

                return (
                  <tr key={inst.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 font-bold text-[#0f1419]">#{inst.id}</td>
                    <td className="px-6 py-4 font-bold text-[#0f1419]">{customer?.name}</td>
                    <td className="px-6 py-4 font-medium text-gray-600">{inst.totalAmount.toLocaleString()} ج.م</td>
                    <td className="px-6 py-4 font-bold text-emerald-600">{inst.paidAmount.toLocaleString()} ج.م</td>
                    <td className="px-6 py-4 font-bold text-[#e11d48]">{inst.remainingAmount.toLocaleString()} ج.م</td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <span>{inst.nextDueDate}</span>
                        {isLate && <AlertCircle size={14} className="text-red-500" title="متأخر" />}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-center gap-2">
                        <button className="px-3 py-1.5 rounded-lg bg-[#0f1419] text-white flex items-center gap-1 hover:bg-black transition-colors text-xs font-bold">
                          <CreditCard size={14} /> سداد دفعة
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
