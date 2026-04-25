import React from 'react';
import { Menu, Search, Bell } from 'lucide-react';

export const Header = () => {
  return (
    <header className="h-16 border-b border-gray-100 bg-white flex items-center justify-between px-6 sticky top-0 z-10">
      <div className="flex items-center gap-4">
        <button className="md:hidden text-[#0f1419] p-2 hover:bg-gray-50 rounded-lg transition-colors">
          <Menu size={24} />
        </button>
        <div className="relative hidden md:block w-96">
          <input 
            type="text" 
            placeholder="ابحث عن أي شيء (Ctrl+K)..." 
            className="w-full bg-gray-50 border border-transparent focus:border-[#0f1419] focus:bg-white rounded-xl py-2 pr-10 pl-4 text-sm outline-none transition-all"
          />
          <Search className="absolute right-3 top-2.5 text-gray-400" size={18} />
        </div>
      </div>
      
      <div className="flex items-center gap-4">
        <button className="relative p-2 text-gray-400 hover:text-[#0f1419] hover:bg-gray-50 rounded-lg transition-colors">
          <Bell size={20} />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-[#e11d48] rounded-full border-2 border-white"></span>
        </button>
        <div className="flex items-center gap-2 cursor-pointer">
          <div className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center font-bold text-[#0f1419]">
            ع
          </div>
          <div className="hidden md:block text-right">
            <p className="text-sm font-bold text-[#0f1419] leading-none">عمرو أكمل</p>
            <p className="text-xs text-gray-500 mt-1">مدير النظام</p>
          </div>
        </div>
      </div>
    </header>
  );
};
