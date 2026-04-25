import React, { useState } from 'react';
import { Plus, Search, Edit2, Trash2, Box, TrendingUp, AlertTriangle } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const Products = () => {
  const { state, addProduct } = useAppContext();
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newProduct, setNewProduct] = useState({ name: '', category: '', quantity: 0, purchasePrice: 0, salePrice: 0, barcode: '' });

  const filteredProducts = state.products.filter(p => p.name.includes(searchTerm) || p.barcode.includes(searchTerm));

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    addProduct(newProduct);
    setIsModalOpen(false);
    setNewProduct({ name: '', category: '', quantity: 0, purchasePrice: 0, salePrice: 0, barcode: '' });
  };

  const totalValue = state.products.reduce((acc, p) => acc + (p.purchasePrice * p.quantity), 0);
  const lowStockCount = state.products.filter(p => p.quantity < 10).length;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <Box className="text-[#0f1419]" /> إدارة المنتجات والمخزون
          </h1>
        </div>
        <button 
          onClick={() => setIsModalOpen(true)}
          className="bg-[#0f1419] text-white px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-black transition-colors"
        >
          <Plus size={18} /> منتج جديد
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-gray-50 flex items-center justify-center text-[#0f1419]">
            <Box />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">إجمالي المنتجات</p>
            <h3 className="text-xl font-bold text-[#0f1419]">{state.products.length}</h3>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-gray-50 flex items-center justify-center text-[#0f1419]">
            <TrendingUp />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">قيمة المخزون</p>
            <h3 className="text-xl font-bold text-[#0f1419]">{totalValue.toLocaleString()} ج.م</h3>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-red-50 flex items-center justify-center text-[#e11d48]">
            <AlertTriangle />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">نواقص المخزون</p>
            <h3 className="text-xl font-bold text-[#e11d48]">{lowStockCount}</h3>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="p-4 border-b border-gray-100 bg-gray-50 flex justify-between items-center">
          <div className="relative w-72">
            <Search className="absolute right-3 top-2.5 text-gray-400" size={18} />
            <input 
              type="text" 
              placeholder="بحث بالاسم أو الباركود..." 
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
                <th className="px-6 py-4 font-bold">المنتج</th>
                <th className="px-6 py-4 font-bold">الفئة</th>
                <th className="px-6 py-4 font-bold">الكمية</th>
                <th className="px-6 py-4 font-bold">سعر الشراء</th>
                <th className="px-6 py-4 font-bold">سعر البيع</th>
                <th className="px-6 py-4 font-bold">الباركود</th>
                <th className="px-6 py-4 font-bold text-center">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50 text-sm">
              {filteredProducts.map(product => (
                <tr key={product.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 font-bold text-[#0f1419]">{product.name}</td>
                  <td className="px-6 py-4 text-gray-500">{product.category}</td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-1 rounded-full font-bold text-xs ${product.quantity < 10 ? 'bg-red-50 text-[#e11d48]' : 'bg-gray-100 text-[#0f1419]'}`}>
                      {product.quantity}
                    </span>
                  </td>
                  <td className="px-6 py-4 font-medium text-gray-600">{product.purchasePrice.toLocaleString()} ج.م</td>
                  <td className="px-6 py-4 font-bold text-[#0f1419]">{product.salePrice.toLocaleString()} ج.م</td>
                  <td className="px-6 py-4 text-gray-400 font-mono text-xs">{product.barcode}</td>
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

      {/* Add Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center backdrop-blur-sm">
          <div className="bg-white rounded-2xl w-full max-w-lg p-6 shadow-2xl">
            <h2 className="text-xl font-bold text-[#0f1419] mb-6">إضافة منتج جديد</h2>
            <form onSubmit={handleAdd} className="space-y-4">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">اسم المنتج</label>
                <input required type="text" value={newProduct.name} onChange={e => setNewProduct({...newProduct, name: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1">الفئة</label>
                  <input required type="text" value={newProduct.category} onChange={e => setNewProduct({...newProduct, category: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1">الباركود</label>
                  <input required type="text" value={newProduct.barcode} onChange={e => setNewProduct({...newProduct, barcode: e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
                </div>
              </div>
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1">الكمية</label>
                  <input required type="number" value={newProduct.quantity} onChange={e => setNewProduct({...newProduct, quantity: +e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1">سعر الشراء</label>
                  <input required type="number" value={newProduct.purchasePrice} onChange={e => setNewProduct({...newProduct, purchasePrice: +e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1">سعر البيع</label>
                  <input required type="number" value={newProduct.salePrice} onChange={e => setNewProduct({...newProduct, salePrice: +e.target.value})} className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none" />
                </div>
              </div>
              <div className="flex gap-3 pt-4">
                <button type="button" onClick={() => setIsModalOpen(false)} className="flex-1 py-2.5 border border-gray-200 text-gray-600 rounded-xl font-bold hover:bg-gray-50">إلغاء</button>
                <button type="submit" className="flex-1 py-2.5 bg-[#0f1419] text-white rounded-xl font-bold hover:bg-black">حفظ المنتج</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
