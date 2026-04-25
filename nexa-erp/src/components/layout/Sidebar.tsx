import React from 'react';
import { NavLink } from 'react-router-dom';
import { 
  LayoutDashboard, ShoppingCart, Box, Users, BarChart3, 
  Settings, Tags, Receipt, CalendarDays, Wallet, MessageCircle, FileText 
} from 'lucide-react';

export const Sidebar = () => {
  const menuItems = [
    { icon: LayoutDashboard, label: 'لوحة التحكم', path: '/' },
    { icon: ShoppingCart, label: 'البيع السريع', path: '/pos' },
    { icon: FileText, label: 'سجل المبيعات', path: '/sales' },
    { icon: Box, label: 'المنتجات', path: '/products' },
    { icon: Tags, label: 'إدارة الفئات', path: '/categories' },
    { icon: Users, label: 'العملاء', path: '/customers' },
    { icon: Wallet, label: 'حساب الخزنة', path: '/store-account' },
    { icon: Receipt, label: 'الديون والمصروفات', path: '/debts' },
    { icon: CalendarDays, label: 'الأقساط', path: '/installments' },
    { icon: MessageCircle, label: 'رسائل الواتساب', path: '/whatsapp' },
    { icon: BarChart3, label: 'التقارير المتقدمة', path: '/reports' },
  ];

  return (
    <aside className="fixed right-0 top-0 h-screen w-64 bg-white border-l border-gray-100 flex flex-col hidden md:flex">
      <div className="p-6 flex items-center gap-3">
        <div className="w-8 h-8 bg-[#0f1419] rounded-lg flex items-center justify-center">
          <span className="text-white font-bold text-xl leading-none">Z</span>
        </div>
        <h1 className="text-xl font-bold text-[#0f1419] tracking-tight">Zenith ERP</h1>
      </div>
      
      <nav className="flex-1 px-4 space-y-1 mt-4 overflow-y-auto no-scrollbar pb-4">
        {menuItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              `flex items-center gap-3 px-4 py-3 rounded-xl transition-all font-medium ${
                isActive 
                  ? 'bg-[#0f1419] text-white shadow-md' 
                  : 'text-gray-500 hover:bg-gray-50 hover:text-[#0f1419]'
              }`
            }
          >
            <item.icon size={20} strokeWidth={2.5} />
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="p-4 border-t border-gray-100">
        <NavLink to="/settings" className={({ isActive }) => `flex items-center gap-3 px-4 py-3 rounded-xl w-full font-medium transition-colors ${isActive ? 'bg-[#0f1419] text-white shadow-md' : 'text-gray-500 hover:bg-gray-50 hover:text-[#0f1419]'}`}>
          <Settings size={20} strokeWidth={2.5} />
          الإعدادات
        </NavLink>
      </div>
    </aside>
  );
};
