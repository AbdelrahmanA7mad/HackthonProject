import React, { useState } from 'react';
import { Plus, Search, Tags, Edit2, Trash2 } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const Categories = () => {
  const { state, addCategory } = useAppContext();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newCategory, setNewCategory] = useState({ name: '', description: '', status: 'نشط' });

  const filteredCategories = state.categories.filter(c => 
    c.name.includes(searchTerm)
  );

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    addCategory(newCategory);
    setIsModalOpen(false);
    setNewCategory({ name: '', description: '', status: 'نشط' });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <Tags className="text-[#0f1419]" /> إدارة الفئات
          </h1>
        </div>
        <button 
          onClick={() => setIsModalOpen(true)}
          className="bg-[#0f1419] text-white px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-black transition-colors"
        >
          <Plus size={18} /> فئة جديدة
        </button>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100 bg-gray-50 flex justify-between items-center">
          <div className="relative w-full md:w-96">
            <Search className="absolute right-3 top-2.5 text-gray-400" size={18} />
            <input 
              type="text" 
              placeholder="ابحث عن فئة..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full bg-white border border-gray-200 focus:border-[#0f1419] rounded-xl py-2 pr-10 pl-4 outline-none transition-all text-sm"
            />
          </div>
          <div className="text-sm font-bold text-gray-500 hidden md:block">
            إجمالي الفئات: {state.categories.length}
          </div>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full text-right">
            <thead className="bg-gray-50 text-gray-500 text-sm border-b border-gray-100">
              <tr>
                <th className="px-6 py-4 font-bold">اسم الفئة</th>
                <th className="px-6 py-4 font-bold">الوصف</th>
                <th className="px-6 py-4 font-bold">الحالة</th>
                <th className="px-6 py-4 font-bold text-center">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50 text-sm">
              {filteredCategories.map(category => (
                <tr key={category.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 font-bold text-[#0f1419]">{category.name}</td>
                  <td className="px-6 py-4 text-gray-500">{category.description || '-'}</td>
                  <td className="px-6 py-4">
                    <span className={`px-3 py-1 rounded-full font-bold text-xs ${category.status === 'نشط' ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-700'}`}>
                      {category.status}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center justify-center gap-2">
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

      {isModalOpen && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center backdrop-blur-sm">
          <div className="bg-white rounded-2xl w-full max-w-md p-6 shadow-2xl">
            <h2 className="text-xl font-bold text-[#0f1419] mb-6">إضافة فئة جديدة</h2>
            <form onSubmit={handleAdd} className="space-y-4">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">اسم الفئة</label>
                <input required type="text" value={newCategory.name} onChange={e => setNewCategory({...newCategory, name: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">الوصف</label>
                <textarea value={newCategory.description} onChange={e => setNewCategory({...newCategory, description: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none h-24" />
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
