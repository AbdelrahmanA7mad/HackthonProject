import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AppProvider } from './store/AppContext';
import { Layout } from './components/layout/Layout';
import { Dashboard } from './pages/Dashboard';
import { QuickSale } from './pages/QuickSale';
import { Products } from './pages/Products';
import { Customers } from './pages/Customers';
import { Reports } from './pages/Reports';
import { Categories } from './pages/Categories';
import { SalesHistory } from './pages/SalesHistory';
import { StoreAccount } from './pages/StoreAccount';
import { GeneralDebts } from './pages/GeneralDebts';
import { Installments } from './pages/Installments';
import { Settings } from './pages/Settings';
import { WhatsApp } from './pages/WhatsApp';

function App() {
  return (
    <AppProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<Dashboard />} />
            <Route path="pos" element={<QuickSale />} />
            <Route path="sales" element={<SalesHistory />} />
            <Route path="products" element={<Products />} />
            <Route path="categories" element={<Categories />} />
            <Route path="customers" element={<Customers />} />
            <Route path="reports" element={<Reports />} />
            <Route path="store-account" element={<StoreAccount />} />
            <Route path="debts" element={<GeneralDebts />} />
            <Route path="installments" element={<Installments />} />
            <Route path="settings" element={<Settings />} />
            <Route path="whatsapp" element={<WhatsApp />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AppProvider>
  );
}

export default App;
