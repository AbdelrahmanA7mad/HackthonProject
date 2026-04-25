import React, { useState } from 'react';
import { MessageCircle, Send, Users, Smartphone } from 'lucide-react';
import { useAppContext } from '../store/AppContext';

export const WhatsApp = () => {
  const { state } = useAppContext();
  const [message, setMessage] = useState('');
  const [targetGroup, setTargetGroup] = useState('all');

  let targetCustomers = state.customers;
  if (targetGroup === 'with_sales') {
    targetCustomers = state.customers.filter(c => c.salesCount > 0);
  } else if (targetGroup === 'no_sales') {
    targetCustomers = state.customers.filter(c => c.salesCount === 0);
  }

  const handleSend = () => {
    if (!message.trim()) return;
    alert(`جاري إرسال الرسالة إلى ${targetCustomers.length} عميل عبر WhatsApp Web...`);
    setMessage('');
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <MessageCircle className="text-[#25D366]" /> رسائل الواتساب
          </h1>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Composition Area */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
            <h3 className="font-bold text-[#0f1419] mb-4 flex items-center gap-2">
              <Send size={18} /> صياغة الرسالة
            </h3>
            <textarea 
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder="اكتب رسالتك هنا... مثال: عرض خاص لعملائنا الكرام بخصم 20% على جميع المنتجات!"
              className="w-full border border-gray-200 rounded-xl p-4 focus:border-[#25D366] focus:ring-1 focus:ring-[#25D366] outline-none transition-all h-48 resize-none"
            />
            
            <div className="mt-4 flex flex-wrap gap-2">
              <button onClick={() => setMessage(prev => prev + ' {اسم_العميل} ')} className="px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg text-xs font-bold text-gray-600 hover:bg-gray-100">
                + اسم العميل
              </button>
              <button onClick={() => setMessage(prev => prev + ' {رقم_الفاتورة} ')} className="px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg text-xs font-bold text-gray-600 hover:bg-gray-100">
                + رقم الفاتورة الأخيرة
              </button>
            </div>
          </div>

          <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-[#25D366]/10 flex items-center justify-center text-[#25D366]">
                <Users size={20} />
              </div>
              <div>
                <p className="text-sm font-bold text-gray-500">سيتم الإرسال إلى</p>
                <h4 className="text-lg font-bold text-[#0f1419]">{targetCustomers.length} عميل</h4>
              </div>
            </div>
            <button 
              onClick={handleSend}
              disabled={!message.trim() || targetCustomers.length === 0}
              className="bg-[#25D366] text-white px-6 py-3 rounded-xl font-bold flex items-center gap-2 hover:bg-[#1da851] transition-colors disabled:opacity-50"
            >
              <Send size={18} /> إرسال للجميع
            </button>
          </div>
        </div>

        {/* Target Audience */}
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
            <h3 className="font-bold text-[#0f1419] mb-4 flex items-center gap-2">
              <Smartphone size={18} /> شريحة العملاء
            </h3>
            <div className="space-y-3">
              <label className={`flex items-center gap-3 p-3 rounded-xl border cursor-pointer transition-colors ${targetGroup === 'all' ? 'border-[#0f1419] bg-gray-50' : 'border-gray-100 hover:bg-gray-50'}`}>
                <input type="radio" name="targetGroup" checked={targetGroup === 'all'} onChange={() => setTargetGroup('all')} className="w-4 h-4 text-[#0f1419] focus:ring-[#0f1419]" />
                <div className="flex-1">
                  <p className="font-bold text-[#0f1419] text-sm">كل العملاء</p>
                  <p className="text-xs text-gray-500">{state.customers.length} عميل مسجل</p>
                </div>
              </label>

              <label className={`flex items-center gap-3 p-3 rounded-xl border cursor-pointer transition-colors ${targetGroup === 'with_sales' ? 'border-[#0f1419] bg-gray-50' : 'border-gray-100 hover:bg-gray-50'}`}>
                <input type="radio" name="targetGroup" checked={targetGroup === 'with_sales'} onChange={() => setTargetGroup('with_sales')} className="w-4 h-4 text-[#0f1419] focus:ring-[#0f1419]" />
                <div className="flex-1">
                  <p className="font-bold text-[#0f1419] text-sm">عملاء لديهم مشتريات</p>
                  <p className="text-xs text-gray-500">{state.customers.filter(c => c.salesCount > 0).length} عميل</p>
                </div>
              </label>

              <label className={`flex items-center gap-3 p-3 rounded-xl border cursor-pointer transition-colors ${targetGroup === 'no_sales' ? 'border-[#0f1419] bg-gray-50' : 'border-gray-100 hover:bg-gray-50'}`}>
                <input type="radio" name="targetGroup" checked={targetGroup === 'no_sales'} onChange={() => setTargetGroup('no_sales')} className="w-4 h-4 text-[#0f1419] focus:ring-[#0f1419]" />
                <div className="flex-1">
                  <p className="font-bold text-[#0f1419] text-sm">عملاء بدون مشتريات</p>
                  <p className="text-xs text-gray-500">{state.customers.filter(c => c.salesCount === 0).length} عميل</p>
                </div>
              </label>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
};
