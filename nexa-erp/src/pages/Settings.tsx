import React from 'react';
import { Settings as SettingsIcon, Save, CreditCard, Store, Bell } from 'lucide-react';

export const Settings = () => {
  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-[#0f1419] flex items-center gap-2">
            <SettingsIcon className="text-[#0f1419]" /> إعدادات النظام
          </h1>
        </div>
        <button className="bg-[#0f1419] text-white px-4 py-2.5 rounded-xl font-bold flex items-center gap-2 hover:bg-black transition-colors">
          <Save size={18} /> حفظ التغييرات
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        
        {/* General Settings */}
        <div className="md:col-span-2 space-y-6">
          <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
            <h3 className="font-bold text-[#0f1419] mb-4 flex items-center gap-2">
              <Store size={18} /> إعدادات المتجر
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">اسم المتجر</label>
                <input type="text" defaultValue="Zenith Corporation" className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none transition-all text-sm" />
              </div>
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1">رقم الهاتف الأساسي</label>
                <input type="text" defaultValue="01012345678" className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none transition-all text-sm" />
              </div>
              <div className="md:col-span-2">
                <label className="block text-sm font-bold text-gray-700 mb-1">العنوان</label>
                <textarea defaultValue="القاهرة، مدينة نصر" className="w-full border border-gray-200 rounded-xl px-4 py-2 focus:border-[#0f1419] outline-none transition-all text-sm h-20" />
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
            <h3 className="font-bold text-[#0f1419] mb-4 flex items-center gap-2">
              <Bell size={18} /> إعدادات الإشعارات
            </h3>
            <div className="space-y-3">
              <label className="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" defaultChecked className="w-4 h-4 text-[#0f1419] rounded border-gray-300 focus:ring-[#0f1419]" />
                <span className="text-sm font-bold text-gray-700">تنبيه عند انخفاض المخزون</span>
              </label>
              <label className="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" defaultChecked className="w-4 h-4 text-[#0f1419] rounded border-gray-300 focus:ring-[#0f1419]" />
                <span className="text-sm font-bold text-gray-700">تنبيه بالأقساط المتأخرة</span>
              </label>
            </div>
          </div>
        </div>

        {/* Payment Methods */}
        <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm h-fit">
          <h3 className="font-bold text-[#0f1419] mb-4 flex items-center gap-2">
            <CreditCard size={18} /> طرق الدفع المفعلة
          </h3>
          <div className="space-y-3">
            {['كاش (نقدي)', 'فيزا / ماستركارد', 'فودافون كاش', 'آجل (دفع لاحق)'].map((method, i) => (
              <div key={i} className="flex items-center justify-between p-3 border border-gray-100 rounded-xl bg-gray-50">
                <span className="text-sm font-bold text-[#0f1419]">{method}</span>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" defaultChecked className="sr-only peer" />
                  <div className="w-9 h-5 bg-gray-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-[#0f1419]"></div>
                </label>
              </div>
            ))}
          </div>
          <button className="w-full mt-4 bg-gray-50 border border-gray-200 text-[#0f1419] py-2 rounded-xl text-sm font-bold hover:bg-gray-100 transition-colors">
            إضافة طريقة دفع جديدة
          </button>
        </div>

      </div>
    </div>
  );
};
