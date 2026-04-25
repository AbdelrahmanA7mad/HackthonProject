import React from 'react';
import { Sparkles, TrendingUp, DollarSign, ShoppingCart, Users } from 'lucide-react';
import { useAppContext } from '../store/AppContext';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export const Dashboard = () => {
  const { state } = useAppContext();
  
  const totalSales = state.sales.reduce((acc, sale) => acc + sale.totalAmount, 0);
  const totalCustomers = state.customers.length;
  const totalProducts = state.products.length;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center mb-2">
        <h1 className="text-2xl font-bold text-[#0f1419]">نظرة عامة</h1>
        <div className="text-sm font-medium text-gray-500">{new Date().toLocaleDateString('ar-EG', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</div>
      </div>

      {/* AI Insight Widget */}
      <div className="bg-[#0f1419] rounded-2xl p-6 text-white shadow-xl relative overflow-hidden">
        <div className="absolute left-0 top-0 w-32 h-32 bg-white opacity-5 rounded-full blur-2xl transform -translate-x-10 -translate-y-10"></div>
        <div className="flex items-start gap-4 relative z-10">
          <div className="w-12 h-12 bg-white/10 rounded-xl flex items-center justify-center shrink-0">
            <Sparkles className="text-white" />
          </div>
          <div>
            <h3 className="font-bold text-lg mb-1 flex items-center gap-2">Zenith AI Insights</h3>
            <p className="text-gray-300 text-sm leading-relaxed max-w-3xl">
              أداء المبيعات ممتاز هذا الشهر! لقد تجاوزت الإيرادات 220,000 ج.م بزيادة 22% عن الشهر الماضي. 
              أنصحك بالتركيز على تسويق "لابتوب ديل XPS" حيث يمثل 40% من حجم المبيعات مؤخراً.
            </p>
          </div>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4 hover:-translate-y-1 transition-transform">
          <div className="w-12 h-12 rounded-xl bg-gray-50 flex items-center justify-center text-[#0f1419]">
            <DollarSign />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">إجمالي الإيرادات</p>
            <h3 className="text-xl font-bold text-[#0f1419]">{totalSales.toLocaleString()} ج.م</h3>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4 hover:-translate-y-1 transition-transform">
          <div className="w-12 h-12 rounded-xl bg-gray-50 flex items-center justify-center text-[#0f1419]">
            <ShoppingCart />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">فواتير البيع</p>
            <h3 className="text-xl font-bold text-[#0f1419]">{state.sales.length} فاتورة</h3>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4 hover:-translate-y-1 transition-transform">
          <div className="w-12 h-12 rounded-xl bg-gray-50 flex items-center justify-center text-[#0f1419]">
            <Users />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">العملاء النشطين</p>
            <h3 className="text-xl font-bold text-[#0f1419]">{totalCustomers} عميل</h3>
          </div>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex items-center gap-4 hover:-translate-y-1 transition-transform">
          <div className="w-12 h-12 rounded-xl bg-gray-50 flex items-center justify-center text-[#e11d48]">
            <TrendingUp />
          </div>
          <div>
            <p className="text-sm font-bold text-gray-500">إجمالي المصروفات</p>
            <h3 className="text-xl font-bold text-[#e11d48]">85,000 ج.م</h3>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
          <h3 className="font-bold text-[#0f1419] mb-6">الإيرادات والمصروفات (شهرياً)</h3>
          <div className="h-72 w-full" dir="ltr">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={state.monthlyRevenue}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                <XAxis dataKey="name" axisLine={false} tickLine={false} />
                <YAxis axisLine={false} tickLine={false} />
                <Tooltip cursor={{ fill: '#f9fafb' }} />
                <Bar dataKey="revenue" fill="#0f1419" radius={[4, 4, 0, 0]} name="الإيرادات" />
                <Bar dataKey="expenses" fill="#e5e7eb" radius={[4, 4, 0, 0]} name="المصروفات" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 flex flex-col">
          <div className="flex justify-between items-center mb-6">
            <h3 className="font-bold text-[#0f1419]">أحدث العمليات</h3>
            <button className="text-sm font-bold text-gray-500 hover:text-[#0f1419]">عرض الكل</button>
          </div>
          <div className="flex-1 overflow-y-auto pr-1 space-y-4">
            {state.sales.slice(0, 5).map(sale => {
              const customer = state.customers.find(c => c.id === sale.customerId);
              return (
                <div key={sale.id} className="flex items-center justify-between pb-4 border-b border-gray-50 last:border-0 last:pb-0">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-xl bg-gray-50 flex items-center justify-center font-bold text-sm text-[#0f1419]">
                      #{sale.id}
                    </div>
                    <div>
                      <p className="text-sm font-bold text-[#0f1419]">{customer?.name || 'عميل نقدي'}</p>
                      <p className="text-xs text-gray-500 mt-0.5">{sale.date}</p>
                    </div>
                  </div>
                  <div className="text-left">
                    <p className="text-sm font-bold text-[#0f1419]">{sale.totalAmount.toLocaleString()} ج.م</p>
                    <p className="text-xs text-gray-500 mt-0.5">{sale.paymentType}</p>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
};
