import React, { useState } from 'react';
import { Bot, X, Send, Sparkles, TrendingUp, DollarSign } from 'lucide-react';

export const AIAssistant = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [query, setQuery] = useState('');
  const [messages, setMessages] = useState([
    { id: 1, text: 'مرحباً! أنا مساعدك الذكي Zenith AI. كيف يمكنني مساعدتك في إدارة نظامك اليوم؟', sender: 'ai' }
  ]);

  const handleSend = () => {
    if (!query.trim()) return;
    
    const newMsg = { id: Date.now(), text: query, sender: 'user' };
    setMessages(prev => [...prev, newMsg]);
    setQuery('');
    
    // Mock AI response
    setTimeout(() => {
      let aiResponse = 'عذراً، لم أفهم طلبك جيداً. هل يمكنك التوضيح؟';
      const lowerQuery = newMsg.text.toLowerCase();
      
      if (lowerQuery.includes('مبيعات') || lowerQuery.includes('المبيعات')) {
        aiResponse = 'مبيعات اليوم جيدة جداً! لقد حققت زيادة بنسبة 12% مقارنة بأمس. هل ترغب في عرض تقرير المبيعات المفصل؟';
      } else if (lowerQuery.includes('عميل') || lowerQuery.includes('اضافة')) {
        aiResponse = 'يمكنك إضافة عميل جديد من خلال قائمة العملاء في القائمة الجانبية، أو يمكنني فتح شاشة الإضافة لك الآن إذا أردت.';
      } else if (lowerQuery.includes('ارباح') || lowerQuery.includes('اربح')) {
        aiResponse = 'بناءً على تحليلاتي، هامش الربح الحالي هو 24%. أنصحك بمراجعة تسعير المنتجات في فئة "الإلكترونيات" لزيادة الأرباح.';
      }

      setMessages(prev => [...prev, { id: Date.now(), text: aiResponse, sender: 'ai' }]);
    }, 800);
  };

  return (
    <>
      <button 
        onClick={() => setIsOpen(true)}
        className={`fixed bottom-6 left-6 w-14 h-14 bg-[#0f1419] text-white rounded-full flex items-center justify-center shadow-lg transition-transform hover:scale-105 z-40 ${!isOpen ? 'ai-pulse' : 'hidden'}`}
      >
        <Sparkles size={24} />
      </button>

      {isOpen && (
        <div className="fixed bottom-6 left-6 w-80 md:w-96 bg-white rounded-2xl shadow-2xl border border-gray-100 flex flex-col z-50 overflow-hidden" style={{ height: '500px', maxHeight: '80vh' }}>
          <div className="bg-[#0f1419] p-4 flex items-center justify-between text-white">
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 bg-white/10 rounded-full flex items-center justify-center">
                <Bot size={20} />
              </div>
              <div>
                <h3 className="font-bold text-sm">Zenith AI Co-Pilot</h3>
                <p className="text-xs text-gray-400">متصل وجاهز للمساعدة</p>
              </div>
            </div>
            <button onClick={() => setIsOpen(false)} className="text-gray-400 hover:text-white transition-colors">
              <X size={20} />
            </button>
          </div>

          <div className="flex-1 p-4 overflow-y-auto bg-gray-50 flex flex-col gap-3">
            {messages.map(msg => (
              <div key={msg.id} className={`flex ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}>
                <div className={`max-w-[85%] p-3 rounded-2xl text-sm leading-relaxed ${msg.sender === 'user' ? 'bg-[#0f1419] text-white rounded-tl-none' : 'bg-white text-gray-700 border border-gray-100 shadow-sm rounded-tr-none'}`}>
                  {msg.text}
                </div>
              </div>
            ))}
          </div>

          <div className="p-3 bg-white border-t border-gray-100">
            <div className="flex gap-2 mb-2 overflow-x-auto pb-1 no-scrollbar">
              <button onClick={() => setQuery('ما هو ملخص المبيعات اليوم؟')} className="whitespace-nowrap px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg text-xs font-medium text-gray-600 hover:bg-gray-100 transition-colors flex items-center gap-1">
                <TrendingUp size={14} /> ملخص المبيعات
              </button>
              <button onClick={() => setQuery('كيف يمكنني زيادة أرباحي؟')} className="whitespace-nowrap px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg text-xs font-medium text-gray-600 hover:bg-gray-100 transition-colors flex items-center gap-1">
                <DollarSign size={14} /> تحسين الأرباح
              </button>
            </div>
            <div className="flex items-center gap-2">
              <input 
                type="text" 
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSend()}
                placeholder="اسأل Zenith AI عن أي شيء..."
                className="flex-1 bg-gray-50 border border-gray-200 focus:border-[#0f1419] rounded-xl px-4 py-2 text-sm outline-none transition-all"
              />
              <button 
                onClick={handleSend}
                disabled={!query.trim()}
                className="w-10 h-10 bg-[#0f1419] text-white rounded-xl flex items-center justify-center disabled:opacity-50 transition-opacity"
              >
                <Send size={18} className="transform -translate-x-0.5" />
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};
