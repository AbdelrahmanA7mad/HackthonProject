import React from 'react';
import { Wallet, ArrowDownToLine, ArrowUpFromLine, RefreshCcw } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const StoreAccount = () => {
  const { state } = useAppContext();
  const { storeAccount } = state;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <Wallet className="text-[#0f1419]" /> حساب الخزنة
          </h1>
        </div>
        <button className="bg-white border border-gray-200 text-gray-700 px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-gray-50 transition-colors">
          <RefreshCcw size={18} /> تحديث الأرصدة
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-[#0f1419] text-white p-6 rounded-2xl shadow-lg relative overflow-hidden">
          <div className="absolute left-0 top-0 w-32 h-32 bg-white opacity-5 rounded-full blur-2xl transform -translate-x-10 -translate-y-10"></div>
          <p className="text-gray-400 font-bold mb-2">رصيد الخزنة الحالي</p>
          <h2 className="text-4xl font-bold">{storeAccount.currentBalance.toLocaleString()} <span className="text-lg font-normal">ج.م</span></h2>
        </div>
        
        <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm flex flex-col justify-center">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-8 h-8 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <ArrowDownToLine size={16} />
            </div>
            <p className="font-bold text-gray-500">إجمالي الوارد</p>
          </div>
          <h3 className="text-2xl font-bold text-[#0f1419]">{storeAccount.totalIncome.toLocaleString()} ج.م</h3>
        </div>

        <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm flex flex-col justify-center">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-8 h-8 rounded-full bg-red-50 text-red-600 flex items-center justify-center">
              <ArrowUpFromLine size={16} />
            </div>
            <p className="font-bold text-gray-500">إجمالي المنصرف</p>
          </div>
          <h3 className="text-2xl font-bold text-[#0f1419]">{storeAccount.totalExpenses.toLocaleString()} ج.م</h3>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden mt-6">
        <div className="p-4 border-b border-gray-100 bg-gray-50">
          <h3 className="font-bold text-[#0f1419]">أحدث الحركات المالية</h3>
        </div>
        <div className="p-6 text-center text-gray-500">
          لا توجد حركات مالية حديثة مسجلة.
        </div>
      </div>
    </div>
  );
};
