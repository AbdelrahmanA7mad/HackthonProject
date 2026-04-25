import React from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { AIAssistant } from '../ai/AIAssistant';

export const Layout = () => {
  return (
    <div className="flex h-screen bg-gray-50 overflow-hidden font-cairo">
      <Sidebar />
      <div className="flex-1 flex flex-col md:mr-64 overflow-hidden transition-all duration-300">
        <Header />
        <main className="flex-1 overflow-x-hidden overflow-y-auto bg-[#fafafa]">
          <div className="container mx-auto p-6 max-w-7xl">
            <Outlet />
          </div>
        </main>
      </div>
      <AIAssistant />
    </div>
  );
};
