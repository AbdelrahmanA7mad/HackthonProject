
import { motion, AnimatePresence } from 'framer-motion';
import { useEffect, useState } from 'react';

export default function Loader() {
  const [isVisible, setIsVisible] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsVisible(false);
    }, 2000);
    return () => clearTimeout(timer);
  }, []);

  return (
    <AnimatePresence>
      {isVisible && (
        <motion.div
          initial={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.8, ease: 'easeInOut' } as any}
          className="fixed inset-0 z-[9999] flex items-center justify-center bg-[#000000]"
        >
          <div className="flex flex-col items-center gap-8">
            <motion.div
              animate={{
                rotate: [0, 360],
                scale: [1, 1.1, 1],
              }}
              transition={{
                duration: 3,
                repeat: Infinity,
                ease: 'easeInOut',
              } as any}
              className="w-24 h-24 bg-[#e4e4e7]/10 rounded-full flex items-center justify-center border-2 border-[#e4e4e7]/20 relative"
            >
              <div className="w-16 h-16 bg-[#e4e4e7] rounded-full flex items-center justify-center shadow-[0_0_40px_rgba(220,252,231,0.3)]">
                 <div className="w-6 h-6 bg-[#000000] rounded-full" />
              </div>
              
              {/* Outer ring */}
              <div className="absolute inset-0 border-2 border-t-[#e4e4e7] rounded-full animate-spin-slow" />
            </motion.div>
            
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.5, duration: 0.8 }}
              className="text-[#e4e4e7] font-bold tracking-[0.3em] text-3xl uppercase"
            >
              Prodmast
            </motion.div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
