import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { initialProducts, initialCustomers, initialSales, monthlyRevenueData, initialCategories, initialDebts, initialInstallments, storeAccountSummary } from '../data/mockData';

type AppState = {
  products: typeof initialProducts;
  customers: typeof initialCustomers;
  sales: typeof initialSales;
  monthlyRevenue: typeof monthlyRevenueData;
  categories: typeof initialCategories;
  debts: typeof initialDebts;
  installments: typeof initialInstallments;
  storeAccount: typeof storeAccountSummary;
};

type AppContextType = {
  state: AppState;
  addProduct: (product: any) => void;
  addCustomer: (customer: any) => void;
  addSale: (sale: any) => void;
  addCategory: (category: any) => void;
  addDebt: (debt: any) => void;
};

const AppContext = createContext<AppContextType | undefined>(undefined);

export const AppProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [state, setState] = useState<AppState>(() => {
    const defaultState = {
      products: initialProducts,
      customers: initialCustomers,
      sales: initialSales,
      monthlyRevenue: monthlyRevenueData,
      categories: initialCategories,
      debts: initialDebts,
      installments: initialInstallments,
      storeAccount: storeAccountSummary,
    };

    const saved = localStorage.getItem('nexa-erp-state');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        return { ...defaultState, ...parsed };
      } catch (e) {
        console.error('Failed to parse state from localStorage');
      }
    }
    return defaultState;
  });

  useEffect(() => {
    localStorage.setItem('nexa-erp-state', JSON.stringify(state));
  }, [state]);

  const addProduct = (product: any) => {
    setState((prev) => ({ ...prev, products: [...prev.products, { ...product, id: Date.now() }] }));
  };

  const addCustomer = (customer: any) => {
    setState((prev) => ({ ...prev, customers: [...prev.customers, { ...customer, id: Date.now(), salesCount: 0 }] }));
  };

  const addSale = (sale: any) => {
    setState((prev) => ({ ...prev, sales: [{ ...sale, id: Date.now() }, ...prev.sales] }));
  };

  const addCategory = (category: any) => {
    setState((prev) => ({ ...prev, categories: [...prev.categories, { ...category, id: Date.now() }] }));
  };

  const addDebt = (debt: any) => {
    setState((prev) => ({ ...prev, debts: [...prev.debts, { ...debt, id: Date.now() }] }));
  };

  return (
    <AppContext.Provider value={{ state, addProduct, addCustomer, addSale, addCategory, addDebt }}>
      {children}
    </AppContext.Provider>
  );
};

export const useAppContext = () => {
  const context = useContext(AppContext);
  if (!context) throw new Error('useAppContext must be used within AppProvider');
  return context;
};
