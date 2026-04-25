import React, { useState } from 'react';
import { Search, Barcode, ShoppingCart, Trash2, Plus, Minus, User, Save, Printer } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const QuickSale = () => {
  const { state, addSale } = useAppContext();
  const [searchTerm, setSearchTerm] = useState('');
  const [cart, setCart] = useState<any[]>([]);
  const [selectedCustomer, setSelectedCustomer] = useState('');
  const [paymentType, setPaymentType] = useState('كاش');

  const filteredProducts = state.products.filter(p => 
    p.name.includes(searchTerm) || p.barcode.includes(searchTerm)
  );

  const addToCart = (product: any) => {
    setCart(prev => {
      const existing = prev.find(item => item.id === product.id);
      if (existing) {
        return prev.map(item => item.id === product.id ? { ...item, qty: item.qty + 1 } : item);
      }
      return [...prev, { ...product, qty: 1 }];
    });
  };

  const updateQty = (id: number, delta: number) => {
    setCart(prev => prev.map(item => {
      if (item.id === id) {
        const newQty = Math.max(1, item.qty + delta);
        return { ...item, qty: newQty };
      }
      return item;
    }));
  };

  const removeFromCart = (id: number) => {
    setCart(prev => prev.filter(item => item.id !== id));
  };

  const total = cart.reduce((acc, item) => acc + (item.salePrice * item.qty), 0);

  const handleCheckout = () => {
    if (cart.length === 0) return;
    
    const newSale = {
      customerId: selectedCustomer ? parseInt(selectedCustomer) : 0,
      date: new Date().toISOString().split('T')[0],
      totalAmount: total,
      paymentType,
      status: paymentType === 'كاش' ? 'خالص' : 'غير مسدد'
    };
    
    addSale(newSale);
    setCart([]);
    alert('تم حفظ البيع بنجاح!');
  };

  return (
    <div className="flex flex-col lg:flex-row gap-6 h-[calc(100vh-8rem)]">
      
      {/* Products Section */}
      <div className="flex-1 flex flex-col gap-4">
        <div className="bg-white p-4 rounded-2xl border border-gray-100 shadow-sm flex gap-4">
          <div className="relative flex-1">
            <Search className="absolute right-3 top-2.5 text-gray-400" size={20} />
            <input 
              type="text" 
              placeholder="بحث بالاسم..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full bg-gray-50 border border-transparent focus:border-[#0f1419] rounded-xl py-2 pr-10 pl-4 outline-none transition-all"
            />
          </div>
          <div className="relative flex-1">
            <Barcode className="absolute right-3 top-2.5 text-gray-400" size={20} />
            <input 
              type="text" 
              placeholder="مسح الباركود..." 
              className="w-full bg-gray-50 border border-transparent focus:border-[#0f1419] rounded-xl py-2 pr-10 pl-4 outline-none transition-all"
            />
          </div>
        </div>

        <div className="flex-1 overflow-y-auto bg-white rounded-2xl border border-gray-100 shadow-sm p-4">
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            {filteredProducts.map(product => (
              <div 
                key={product.id} 
                onClick={() => addToCart(product)}
                className="border border-gray-100 rounded-xl p-4 cursor-pointer hover:border-[#0f1419] hover:shadow-md transition-all group text-center flex flex-col justify-between"
              >
                <div>
                  <h3 className="font-bold text-[#0f1419] text-sm mb-1 line-clamp-2">{product.name}</h3>
                  <p className="text-xs text-gray-400">{product.category}</p>
                </div>
                <div className="mt-3 font-bold text-lg text-[#0f1419]">
                  {product.salePrice.toLocaleString()} <span className="text-xs font-normal">ج.م</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Cart & Checkout Section */}
      <div className="w-full lg:w-96 flex flex-col gap-4">
        <div className="bg-white rounded-2xl border border-gray-100 shadow-sm flex flex-col h-full overflow-hidden">
          
          <div className="p-4 border-b border-gray-100 bg-gray-50 flex items-center gap-2">
            <User size={18} className="text-[#0f1419]" />
            <select 
              className="flex-1 bg-transparent border-none outline-none text-sm font-bold text-[#0f1419] cursor-pointer"
              value={selectedCustomer}
              onChange={(e) => setSelectedCustomer(e.target.value)}
            >
              <option value="">عميل نقدي (بدون تسجيل)</option>
              {state.customers.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div className="flex-1 overflow-y-auto p-4 space-y-3">
            {cart.length === 0 ? (
              <div className="h-full flex flex-col items-center justify-center text-gray-400">
                <ShoppingCart size={48} className="mb-4 opacity-20" />
                <p>السلة فارغة</p>
              </div>
            ) : (
              cart.map(item => (
                <div key={item.id} className="flex items-center justify-between border-b border-gray-50 pb-3 last:border-0">
                  <div className="flex-1">
                    <p className="text-sm font-bold text-[#0f1419] truncate pr-2">{item.name}</p>
                    <p className="text-xs text-gray-500 pr-2 mt-1">{item.salePrice.toLocaleString()} ج.م</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="flex items-center bg-gray-50 rounded-lg border border-gray-100">
                      <button onClick={() => updateQty(item.id, 1)} className="w-7 h-7 flex items-center justify-center hover:bg-gray-200 rounded-r-lg transition-colors"><Plus size={14} /></button>
                      <span className="w-8 text-center text-sm font-bold">{item.qty}</span>
                      <button onClick={() => updateQty(item.id, -1)} className="w-7 h-7 flex items-center justify-center hover:bg-gray-200 rounded-l-lg transition-colors"><Minus size={14} /></button>
                    </div>
                    <button onClick={() => removeFromCart(item.id)} className="w-8 h-8 flex items-center justify-center text-gray-400 hover:text-[#e11d48] hover:bg-red-50 rounded-lg transition-colors">
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          <div className="p-4 bg-gray-50 border-t border-gray-100">
            <div className="flex justify-between items-center mb-4">
              <span className="text-gray-500 font-bold">الإجمالي</span>
              <span className="text-2xl font-bold text-[#0f1419]">{total.toLocaleString()} <span className="text-sm">ج.م</span></span>
            </div>
            
            <div className="grid grid-cols-2 gap-2 mb-4">
               <button 
                 onClick={() => setPaymentType('كاش')}
                 className={`py-2 rounded-xl text-sm font-bold border transition-colors ${paymentType === 'كاش' ? 'bg-[#0f1419] text-white border-[#0f1419]' : 'bg-white text-gray-500 border-gray-200 hover:bg-gray-50'}`}
               >كاش</button>
               <button 
                 onClick={() => setPaymentType('آجل')}
                 className={`py-2 rounded-xl text-sm font-bold border transition-colors ${paymentType === 'آجل' ? 'bg-[#0f1419] text-white border-[#0f1419]' : 'bg-white text-gray-500 border-gray-200 hover:bg-gray-50'}`}
               >آجل</button>
            </div>

            <div className="flex gap-2">
              <button 
                onClick={handleCheckout}
                disabled={cart.length === 0}
                className="flex-1 bg-[#0f1419] text-white py-3 rounded-xl font-bold flex items-center justify-center gap-2 hover:bg-black transition-colors disabled:opacity-50"
              >
                <Save size={18} /> حفظ
              </button>
              <button 
                disabled={cart.length === 0}
                className="w-14 bg-white border border-gray-200 text-[#0f1419] py-3 rounded-xl font-bold flex items-center justify-center hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                <Printer size={18} />
              </button>
            </div>
          </div>

        </div>
      </div>

    </div>
  );
};
