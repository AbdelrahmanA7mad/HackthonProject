import React, { useState } from 'react';
import { BarChart3, TrendingUp, DollarSign, Package, Sparkles, FileSpreadsheet, AlertTriangle, Lightbulb } from 'lucide-react';
import { useAppContext } from '../store/AppContext';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, PieChart, Pie, Cell } from 'recharts';

const COLORS = ['#0f1419', '#4f46e5', '#e11d48', '#10b981', '#f59e0b'];

export const Reports = () => {
  const { state } = useAppContext();
  const [activeTab, setActiveTab] = useState<'financial' | 'sales' | 'inventory'>('financial');

  // Prepare Data
  const categoryData = state.products.reduce((acc: any, p) => {
    const existing = acc.find((c: any) => c.name === p.category);
    if (existing) {
      existing.value += p.quantity;
    } else {
      acc.push({ name: p.category, value: p.quantity });
    }
    return acc;
  }, []);

  const paymentMethodData = state.sales.reduce((acc: any, s) => {
    const existing = acc.find((p: any) => p.name === s.paymentType);
    if (existing) {
      existing.value += s.totalAmount;
    } else {
      acc.push({ name: s.paymentType, value: s.totalAmount });
    }
    return acc;
  }, []);

  // For LineChart: we need a single array where historical lines stop and forecast lines begin.
  // We can do this by having both `revenue` and `forecastRevenue` in the objects.
  // To make the line connect, the last historical point should also have `forecastRevenue: its revenue`.
  const historicalData = state.monthlyRevenue.filter(d => !d.isForecast);
  const forecastData = state.monthlyRevenue.filter(d => d.isForecast);
  
  const lastHistorical = historicalData[historicalData.length - 1];
  
  const combinedChartData = state.monthlyRevenue.map(d => {
    if (!d.isForecast) {
      // For the last historical point, we add it as the start of the forecast line
      if (d.name === lastHistorical.name) {
        return { ...d, forecastRevenue: d.revenue, forecastExpenses: d.expenses };
      }
      return d;
    }
    return d;
  });

  const handleExport = () => {
    alert("هذه نسخة تجريبية (MVP). ميزة تصدير Excel سيتم تفعيلها في النسخة النهائية.");
  };

  const AIAdviceWidget = ({ title, advice, type = 'info' }: { title: string, advice: string, type?: 'info' | 'warning' | 'success' }) => {
    const icons = {
      info: <Sparkles className="text-indigo-400" />,
      warning: <AlertTriangle className="text-amber-400" />,
      success: <Lightbulb className="text-emerald-400" />
    };
    const bgs = {
      info: 'bg-[#0f1419]',
      warning: 'bg-[#0f1419]',
      success: 'bg-[#0f1419]'
    };

    return (
      <div className={`${bgs[type]} rounded-2xl p-6 text-white shadow-xl relative overflow-hidden mb-6`}>
        <div className="absolute left-0 top-0 w-32 h-32 bg-white opacity-5 rounded-full blur-2xl transform -translate-x-10 -translate-y-10"></div>
        <div className="flex items-start gap-4 relative z-10">
          <div className="w-12 h-12 bg-white/10 rounded-xl flex items-center justify-center shrink-0">
            {icons[type]}
          </div>
          <div>
            <h3 className="font-bold text-lg mb-1 flex items-center gap-2">Zenith AI Analyst</h3>
            <h4 className="text-sm text-gray-300 font-bold mb-2">{title}</h4>
            <p className="text-gray-300 text-sm leading-relaxed max-w-4xl">
              {advice}
            </p>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <BarChart3 className="text-[#0f1419]" /> مركز التحليلات والتقارير
          </h1>
          <p className="text-gray-500 text-sm mt-1">مدعوم بالذكاء الاصطناعي للتنبؤ المستقبلي</p>
        </div>
        <button onClick={handleExport} className="bg-white border border-gray-200 text-[#0f1419] px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-gray-50 transition-colors shadow-sm">
          <FileSpreadsheet size={18} className="text-emerald-600" /> تصدير Excel
        </button>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 overflow-x-auto pb-2 no-scrollbar border-b border-gray-200">
        <button 
          onClick={() => setActiveTab('financial')}
          className={`px-6 py-3 font-bold text-sm transition-colors relative whitespace-nowrap ${activeTab === 'financial' ? 'text-[#0f1419]' : 'text-gray-500 hover:text-[#0f1419]'}`}
        >
          التقارير المالية
          {activeTab === 'financial' && <span className="absolute bottom-0 left-0 w-full h-0.5 bg-[#0f1419] rounded-t-full"></span>}
        </button>
        <button 
          onClick={() => setActiveTab('sales')}
          className={`px-6 py-3 font-bold text-sm transition-colors relative whitespace-nowrap ${activeTab === 'sales' ? 'text-[#0f1419]' : 'text-gray-500 hover:text-[#0f1419]'}`}
        >
          تقارير المبيعات
          {activeTab === 'sales' && <span className="absolute bottom-0 left-0 w-full h-0.5 bg-[#0f1419] rounded-t-full"></span>}
        </button>
        <button 
          onClick={() => setActiveTab('inventory')}
          className={`px-6 py-3 font-bold text-sm transition-colors relative whitespace-nowrap ${activeTab === 'inventory' ? 'text-[#0f1419]' : 'text-gray-500 hover:text-[#0f1419]'}`}
        >
          تقارير المخزون
          {activeTab === 'inventory' && <span className="absolute bottom-0 left-0 w-full h-0.5 bg-[#0f1419] rounded-t-full"></span>}
        </button>
      </div>

      {/* Tab Content */}
      <div className="pt-2">
        
        {/* FINANCIAL TAB */}
        {activeTab === 'financial' && (
          <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            
            <AIAdviceWidget 
              type="success"
              title="توقعات الإيرادات والمصروفات"
              advice="بناءً على تحليلي للنمو التاريخي بنسبة 15% شهرياً، أتوقع أن تصل الإيرادات في شهر يونيو إلى 290,000 ج.م. أنصحك بتقليل المصروفات الإدارية للحفاظ على هامش ربح يتجاوز 60%. لقد قمت برسم التوقعات بخطوط متقطعة في الرسم البياني أدناه."
            />

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div className="lg:col-span-2 bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
                <h3 className="font-bold text-[#0f1419] mb-6 flex items-center gap-2">
                  <TrendingUp size={18} /> الإيرادات والمصروفات (الفعلي والمتوقع)
                </h3>
                <div className="h-80 w-full" dir="ltr">
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={combinedChartData}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                      <XAxis dataKey="name" axisLine={false} tickLine={false} />
                      <YAxis axisLine={false} tickLine={false} />
                      <Tooltip cursor={{ fill: '#f9fafb' }} />
                      
                      {/* Historical Lines */}
                      <Line type="monotone" dataKey="revenue" stroke="#0f1419" strokeWidth={3} name="الإيرادات الفعلية" dot={{ r: 4 }} activeDot={{ r: 6 }} />
                      <Line type="monotone" dataKey="expenses" stroke="#e11d48" strokeWidth={3} name="المصروفات الفعلية" dot={{ r: 4 }} activeDot={{ r: 6 }} />
                      
                      {/* Forecast Lines (Dotted) */}
                      <Line type="monotone" dataKey="forecastRevenue" stroke="#0f1419" strokeWidth={3} strokeDasharray="5 5" name="توقع الإيرادات" dot={{ r: 4 }} activeDot={{ r: 6 }} />
                      <Line type="monotone" dataKey="forecastExpenses" stroke="#e11d48" strokeWidth={3} strokeDasharray="5 5" name="توقع المصروفات" dot={{ r: 4 }} activeDot={{ r: 6 }} />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              </div>

              <div className="lg:col-span-2 bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
                <h3 className="font-bold text-[#0f1419] mb-6 flex items-center gap-2">
                  <DollarSign size={18} /> تحليل صافي الربح الشهري
                </h3>
                <div className="h-80 w-full" dir="ltr">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={historicalData}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                      <XAxis dataKey="name" axisLine={false} tickLine={false} />
                      <YAxis axisLine={false} tickLine={false} />
                      <Tooltip cursor={{ fill: '#f9fafb' }} />
                      <Bar dataKey={(data) => data.revenue - data.expenses} fill="#4f46e5" radius={[4, 4, 0, 0]} name="صافي الربح" />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* SALES TAB */}
        {activeTab === 'sales' && (
          <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            
            <AIAdviceWidget 
              type="info"
              title="تحليل قنوات المبيعات"
              advice="لاحظت أن معظم المبيعات تتم عبر طريقة 'الدفع النقدي' (كاش). قد يكون من المفيد إطلاق حملة ترويجية لتشجيع العملاء على استخدام الدفع الآجل للمشتريات الكبيرة لزيادة حجم السلة الشرائية. كما أنصحك بالاستثمار في إعلانات منتجات الفئة 'إلكترونيات' لأنها تحقق أعلى عائد."
            />

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
                <h3 className="font-bold text-[#0f1419] mb-6 flex items-center gap-2">
                  <Package size={18} /> المبيعات حسب طرق الدفع
                </h3>
                <div className="h-80 w-full flex items-center justify-center" dir="ltr">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={paymentMethodData}
                        cx="50%"
                        cy="50%"
                        innerRadius={80}
                        outerRadius={120}
                        paddingAngle={5}
                        dataKey="value"
                        label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                      >
                        {paymentMethodData.map((entry: any, index: number) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </div>

              <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
                <h3 className="font-bold text-[#0f1419] mb-6 flex items-center gap-2">
                  <TrendingUp size={18} /> أفضل العملاء مبيعاً
                </h3>
                <div className="space-y-4">
                  {state.customers.sort((a,b) => b.salesCount - a.salesCount).slice(0, 5).map((customer, idx) => (
                    <div key={customer.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-xl">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-[#0f1419] text-white flex items-center justify-center font-bold text-xs">
                          {idx + 1}
                        </div>
                        <p className="font-bold text-[#0f1419] text-sm">{customer.name}</p>
                      </div>
                      <span className="font-bold text-gray-500 text-sm">{customer.salesCount} فاتورة</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* INVENTORY TAB */}
        {activeTab === 'inventory' && (
          <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            
            <AIAdviceWidget 
              type="warning"
              title="تحذيرات وتنبيهات المخزون"
              advice="تنبيه: منتج 'طابعة إتش بي' على وشك النفاذ (الكمية المتبقية 8 فقط). يجب عليك طلب كميات جديدة من المورد خلال 3 أيام. في المقابل، نلاحظ أن منتج 'ماوس لاسلكي لوجيتك' بطيء الحركة (الكمية 50) ولم يتم بيع الكثير منه مؤخراً؛ أنصحك بعمل خصم 10% لتسريع حركة البيع."
            />

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
                <h3 className="font-bold text-[#0f1419] mb-6 flex items-center gap-2">
                  <Package size={18} /> توزيع المخزون حسب الفئات
                </h3>
                <div className="h-80 w-full flex items-center justify-center" dir="ltr">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={categoryData}
                        cx="50%"
                        cy="50%"
                        innerRadius={80}
                        outerRadius={120}
                        paddingAngle={5}
                        dataKey="value"
                        label={({ name, value }) => `${name} (${value})`}
                      >
                        {categoryData.map((entry: any, index: number) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </div>

              <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
                <h3 className="font-bold text-[#0f1419] mb-6 flex items-center gap-2">
                  <AlertTriangle size={18} /> نواقص المخزون (أقل من 15 قطعة)
                </h3>
                <div className="space-y-4">
                  {state.products.filter(p => p.quantity < 15).map(product => (
                    <div key={product.id} className="flex items-center justify-between p-3 border border-red-100 bg-red-50 rounded-xl">
                      <div>
                        <p className="font-bold text-[#0f1419] text-sm">{product.name}</p>
                        <p className="text-xs text-gray-500">{product.category}</p>
                      </div>
                      <span className="font-bold text-[#e11d48] px-3 py-1 bg-white rounded-lg text-sm border border-red-100">
                        متبقي {product.quantity}
                      </span>
                    </div>
                  ))}
                  {state.products.filter(p => p.quantity < 15).length === 0 && (
                    <p className="text-center text-gray-500 py-4">لا توجد نواقص حالياً</p>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

      </div>
    </div>
  );
};
