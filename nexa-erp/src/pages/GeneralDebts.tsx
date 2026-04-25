import React, { useState } from 'react';
import { Receipt, Plus, Search, CheckCircle, Clock } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const GeneralDebts = () => {
  const { state, addDebt } = useAppContext();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newDebt, setNewDebt] = useState({ name: '', amount: 0, date: '', notes: '', status: 'غير مسدد' });

  const filteredDebts = state.debts.filter(d => 
    d.name.includes(searchTerm)
  );

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    addDebt(newDebt);
    setIsModalOpen(false);
    setNewDebt({ name: '', amount: 0, date: '', notes: '', status: 'غير مسدد' });
  };

  const totalUnpaid = state.debts.filter(d => d.status === 'غير مسدد').reduce((acc, d) => acc + d.amount, 0);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <Receipt className="text-[#0f1419]" /> الديون العامة والمصروفات
          </h1>
        </div>
        <button 
          onClick={() => setIsModalOpen(true)}
          className="bg-[#0f1419] text-white px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-black transition-colors"
        >
          <Plus size={18} /> إضافة دين / مصروف
        </button>
      </div>

      <div className="bg-white p-6 rounded-2xl border border-red-100 shadow-sm flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-red-50 flex items-center justify-center text-red-600">
            <Clock size={24} />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">إجمالي الديون غير المسددة</p>
            <h3 className="text-xl font-bold text-[#e11d48]">{totalUnpaid.toLocaleString()} ج.م</h3>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100 bg-gray-50 flex justify-between items-center">
          <div className="relative w-full md:w-96">
            <Search className="absolute right-3 top-2.5 text-gray-400" size={18} />
            <input 
              type="text" 
              placeholder="ابحث بالاسم أو البيان..." 
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
                <th className="px-6 py-4 font-bold">الجهة / البيان</th>
                <th className="px-6 py-4 font-bold">المبلغ</th>
                <th className="px-6 py-4 font-bold">التاريخ</th>
                <th className="px-6 py-4 font-bold">ملاحظات</th>
                <th className="px-6 py-4 font-bold">الحالة</th>
                <th className="px-6 py-4 font-bold text-center">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50 text-sm">
              {filteredDebts.map(debt => (
                <tr key={debt.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 font-bold text-[#0f1419]">{debt.name}</td>
                  <td className="px-6 py-4 font-bold text-[#0f1419]">{debt.amount.toLocaleString()} ج.م</td>
                  <td className="px-6 py-4 text-gray-500">{debt.date}</td>
                  <td className="px-6 py-4 text-gray-500">{debt.notes || '-'}</td>
                  <td className="px-6 py-4">
                    <span className={`px-3 py-1 rounded-full font-bold text-xs ${debt.status === 'مسدد' ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-700'}`}>
                      {debt.status}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center justify-center gap-2">
                      {debt.status !== 'مسدد' && (
                        <button className="px-3 py-1.5 rounded-lg bg-[#0f1419] text-white flex items-center gap-1 hover:bg-black transition-colors text-xs font-bold">
                          <CheckCircle size={14} /> سداد
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center backdrop-blur-sm">
          <div className="bg-white rounded-2xl w-full max-w-md p-6 shadow-2xl">
            <h2 className="text-xl font-bold text-[#0f1419] mb-6">إضافة دين / مصروف</h2>
            <form onSubmit={handleAdd} className="space-y-4">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">الجهة أو الدائن</label>
                <input required type="text" value={newDebt.name} onChange={e => setNewDebt({...newDebt, name: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1">المبلغ (ج.م)</label>
                  <input required type="number" value={newDebt.amount} onChange={e => setNewDebt({...newDebt, amount: +e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1">التاريخ</label>
                  <input required type="date" value={newDebt.date} onChange={e => setNewDebt({...newDebt, date: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">ملاحظات</label>
                <textarea value={newDebt.notes} onChange={e => setNewDebt({...newDebt, notes: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none h-20" />
              </div>
              <div className="flex gap-3 pt-4 border-t border-gray-100">
                <button type="button" onClick={() => setIsModalOpen(false)} className="flex-1 py-2.5 border border-gray-200 text-gray-600 rounded-xl font-bold hover:bg-gray-50">إلغاء</button>
                <button type="submit" className="flex-1 py-2.5 bg-[#0f1419] text-white rounded-xl font-bold hover:bg-black">حفظ</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
