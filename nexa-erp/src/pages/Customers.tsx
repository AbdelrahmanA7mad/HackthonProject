import React, { useState } from 'react';
import { Plus, Search, Users, UserPlus, FileSpreadsheet, Eye, Edit2, Trash2 } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const Customers = () => {
  const { state, addCustomer } = useAppContext();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newCustomer, setNewCustomer] = useState({ name: '', phone: '', address: '' });

  const filteredCustomers = state.customers.filter(c => 
    c.name.includes(searchTerm) || c.phone.includes(searchTerm)
  );

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    addCustomer(newCustomer);
    setIsModalOpen(false);
    setNewCustomer({ name: '', phone: '', address: '' });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <Users className="text-[#0f1419]" /> إدارة العملاء
          </h1>
        </div>
        <div className="flex gap-3">
          <button className="bg-white border border-gray-200 text-gray-700 px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-gray-50 transition-colors hidden md:flex">
            <FileSpreadsheet size={18} /> تصدير Excel
          </button>
          <button 
            onClick={() => setIsModalOpen(true)}
            className="bg-[#0f1419] text-white px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-black transition-colors"
          >
            <UserPlus size={18} /> إضافة عميل
          </button>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100 bg-gray-50 flex justify-between items-center">
          <div className="relative w-full md:w-96">
            <Search className="absolute right-3 top-2.5 text-gray-400" size={18} />
            <input 
              type="text" 
              placeholder="ابحث بالاسم أو رقم الهاتف..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full bg-white border border-gray-200 focus:border-[#0f1419] rounded-xl py-2 pr-10 pl-4 outline-none transition-all text-sm"
            />
          </div>
          <div className="text-sm font-bold text-gray-500 hidden md:block">
            إجمالي العملاء: {state.customers.length}
          </div>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full text-right">
            <thead className="bg-gray-50 text-gray-500 text-sm border-b border-gray-100">
              <tr>
                <th className="px-6 py-4 font-bold">الاسم الكامل</th>
                <th className="px-6 py-4 font-bold">رقم الهاتف</th>
                <th className="px-6 py-4 font-bold">العنوان</th>
                <th className="px-6 py-4 font-bold">المبيعات</th>
                <th className="px-6 py-4 font-bold text-center">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50 text-sm">
              {filteredCustomers.map(customer => (
                <tr key={customer.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 font-bold text-[#0f1419]">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center font-bold text-[#0f1419] text-xs">
                        {customer.name.substring(0, 1)}
                      </div>
                      {customer.name}
                    </div>
                  </td>
                  <td className="px-6 py-4 font-mono text-gray-600">{customer.phone}</td>
                  <td className="px-6 py-4 text-gray-500">{customer.address || '-'}</td>
                  <td className="px-6 py-4">
                    <span className="px-3 py-1 bg-gray-100 text-[#0f1419] rounded-lg font-bold border border-gray-200">
                      {customer.salesCount}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center justify-center gap-2">
                      <button className="w-8 h-8 rounded-lg bg-gray-50 text-[#0f1419] flex items-center justify-center hover:bg-gray-200 transition-colors">
                        <Eye size={14} />
                      </button>
                      <button className="w-8 h-8 rounded-lg bg-gray-50 text-[#0f1419] flex items-center justify-center hover:bg-gray-200 transition-colors">
                        <Edit2 size={14} />
                      </button>
                      <button className="w-8 h-8 rounded-lg bg-red-50 text-[#e11d48] flex items-center justify-center hover:bg-red-100 transition-colors">
                        <Trash2 size={14} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Add Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center backdrop-blur-sm">
          <div className="bg-white rounded-2xl w-full max-w-md p-6 shadow-2xl">
            <h2 className="text-xl font-bold text-[#0f1419] mb-6">إضافة عميل جديد</h2>
            <form onSubmit={handleAdd} className="space-y-4">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">الاسم الكامل</label>
                <input required type="text" value={newCustomer.name} onChange={e => setNewCustomer({...newCustomer, name: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">رقم الهاتف</label>
                <input required type="text" value={newCustomer.phone} onChange={e => setNewCustomer({...newCustomer, phone: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">العنوان</label>
                <input required type="text" value={newCustomer.address} onChange={e => setNewCustomer({...newCustomer, address: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
              </div>
              <div className="flex gap-3 pt-4 border-t border-gray-100">
                <button type="button" onClick={() => setIsModalOpen(false)} className="flex-1 py-2.5 border border-gray-200 text-gray-600 rounded-xl font-bold hover:bg-gray-50">إلغاء</button>
                <button type="submit" className="flex-1 py-2.5 bg-[#0f1419] text-white rounded-xl font-bold hover:bg-black">حفظ العميل</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
