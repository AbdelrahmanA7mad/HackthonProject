import { motion } from 'framer-motion';
import { Star, ArrowUpRight, LayoutDashboard, Zap, Activity, Settings, BarChart3, Waves, MousePointer2, MoreHorizontal } from 'lucide-react';

const containerVariants = {
  hidden: { opacity: 0 },
  show: {
    opacity: 1,
    transition: {
      staggerChildren: 0.25,
      delayChildren: 3,
    },
  },
};

const itemVariants = {
  hidden: { y: 60, opacity: 0 },
  show: { 
    y: 0, 
    opacity: 1,
    transition: {
      duration: 1.5,
      ease: [0.16, 1, 0.3, 1] as any,
    },
  },
};

const floatingAnimation: any = {
  y: [0, -8, 0],
  transition: {
    duration: 4,
    repeat: Infinity,
    ease: "easeInOut"
  }
};

export default function HeroSection() {
  return (
    <section id="home" className="relative pt-32 pb-32 px-10 max-w-[1440px] mx-auto min-h-screen">
      {/* Decorative Background Elements */}
      <div className="absolute top-60 left-20 w-[600px] h-[600px] bg-[#dcfce7]/20 blur-[150px] rounded-full -z-10" />
      
      {/* Floating Icons - Left Arc */}
      <motion.div animate={floatingAnimation} className="absolute left-[10%] top-[34%] bg-[#0a2a2a] p-2 rounded-full shadow-xl hidden lg:block z-20 -translate-y-[15px]">
        <Settings className="w-4 h-4 text-[#dcfce7]" />
      </motion.div>
      <motion.div animate={{ y: [0, 8, 0], transition: { duration: 5, repeat: Infinity, ease: "easeInOut" } } as any} className="absolute left-[6%] top-[37%] bg-white border border-gray-100 p-2 rounded-full shadow-lg hidden lg:block z-20 -translate-y-[15px]">
        <MousePointer2 className="w-4 h-4 text-[#0a2a2a]" />
      </motion.div>
      <motion.div animate={floatingAnimation} transition={{ delay: 1 }} className="absolute left-[10%] top-[40%] bg-[#dcfce7] p-2 rounded-full shadow-md hidden lg:block z-20 -translate-y-[15px]">
        <div className="w-2.5 h-2.5 bg-[#0a2a2a] rounded-sm" />
      </motion.div>

      {/* Floating Icons - Right Arc */}
      <motion.div animate={floatingAnimation} className="absolute right-[10%] top-[38%] bg-white border border-gray-100 p-2.5 rounded-full shadow-xl hidden lg:block z-20 -translate-y-[15px]">
        <Waves className="w-5 h-5 text-[#0a2a2a]/40" />
      </motion.div>
      <motion.div animate={{ y: [0, 12, 0], transition: { duration: 4.5, repeat: Infinity, ease: "easeInOut" } } as any} className="absolute right-[6%] top-[35%] bg-[#dcfce7] p-2 rounded-full shadow-lg hidden lg:block z-20 -translate-y-[15px]">
        <BarChart3 className="w-4 h-4 text-[#0a2a2a]" />
      </motion.div>

      {/* Content Header */}
      <div className="flex flex-col items-center text-center mb-0 relative z-10">
        <div className="flex flex-col items-center gap-7 mb-10">
          <div className="flex flex-col items-center gap-3.5">
            <motion.h1
              initial={{ y: 80, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              transition={{ duration: 1.8, ease: [0.16, 1, 0.3, 1] as any, delay: 2.2 }}
              className="text-4xl md:text-[65px] font-bold text-[#0a2a2a] leading-[1.1] tracking-tight max-w-[900px] flex flex-col items-center"
            >
              <span className="block">The Future of Manufacturing</span>
              <span className="block text-transparent bg-clip-text bg-gradient-to-r from-[#0a2a2a] to-[#1a4a4a]">
                with Latest Technology
              </span>
            </motion.h1>

            <motion.p
              initial={{ y: 30, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              transition={{ duration: 1.5, ease: [0.16, 1, 0.3, 1] as any, delay: 2.5 }}
              className="text-[#0a2a2a]/60 text-base md:text-lg font-medium max-w-[700px] leading-relaxed"
            >
              Elevate your manufacturing through smart automation and real-time analytics.
            </motion.p>
          </div>

          <motion.div
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ duration: 0.8, delay: 3.2 }}
            className="flex flex-wrap justify-center gap-4.5"
          >
            <button className="bg-[#0a2a2a] text-white px-9 py-3 rounded-full font-bold text-base flex items-center gap-2.5 hover:scale-105 transition-all shadow-xl shadow-[#0a2a2a]/10">
              Get Started
              <div className="bg-white/20 p-1.5 rounded-full">
                <ArrowUpRight className="w-3.5 h-3.5 text-white" />
              </div>
            </button>
            <button className="bg-white text-[#0a2a2a] border-2 border-[#0a2a2a]/5 px-9 py-3 rounded-full font-bold text-base hover:bg-gray-50 transition-all">
              Try Demo
            </button>
          </motion.div>
        </div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 2.5, duration: 1 }}
          className="flex flex-col items-center gap-1 mb-0"
        >
          <div className="flex gap-1 items-center">
            {[1, 2, 3, 4, 5].map((s) => (
              <Star key={s} className="w-3.5 h-3.5 fill-yellow-400 text-yellow-400" />
            ))}
            <span className="ml-1.5 font-bold text-sm text-[#0a2a2a]">5.0</span>
          </div>
          <p className="text-[#0a2a2a]/30 font-bold text-[10px] uppercase tracking-[0.25em]">from 80+ reviews</p>
        </motion.div>
      </div>

      {/* Grid Layout Raised Up */}
      <motion.div
        variants={containerVariants}
        initial="hidden"
        animate="show"
        className="grid grid-cols-1 md:grid-cols-24 gap-8 h-auto items-end -mt-[86px]"
      >
        {/* Card 1: Edge (350px) */}
        <motion.div variants={itemVariants} className="md:col-span-5 h-[350px] rounded-2xl overflow-hidden relative shadow-xl">
           <img 
            src="/img/pipes.png" 
            alt="Manufacturing Pipes" 
            className="absolute inset-0 w-full h-full object-cover"
           />
        </motion.div>

        {/* Card 2: Narrow (280px) */}
        <motion.div variants={itemVariants} className="md:col-span-4 h-[280px] bg-[#0a2a2a] rounded-xl p-8 flex flex-col justify-center items-center text-center text-white shadow-xl">
           <h3 className="text-4xl font-bold mb-2 tracking-tighter">100+</h3>
           <p className="text-white/40 text-[12px] font-bold uppercase tracking-widest leading-tight">Our Esteemed <br/> Clients</p>
        </motion.div>

        {/* Card 3: Middle (210px) */}
        <motion.div variants={itemVariants} className="md:col-span-6 h-[210px] bg-white border border-[#0a2a2a]/10 rounded-xl p-6 shadow-[0_20px_60px_rgba(0,0,0,0.03)] flex flex-col justify-between">
           <div className="flex items-center justify-between">
              <div className="bg-[#dcfce7] p-2 rounded-2xl shadow-sm">
                <LayoutDashboard className="w-5 h-5 text-[#0a2a2a]" />
              </div>
              <div className="flex items-center gap-2">
                <div className="flex items-center gap-1.5">
                  <div className="w-1.5 h-1.5 bg-[#0a2a2a] rounded-full" />
                  <span className="text-[9px] font-bold text-[#0a2a2a] uppercase tracking-wider">8% Inc</span>
                </div>
              </div>
           </div>
           <div>
              <p className="text-[#0a2a2a]/40 font-bold text-[9px] mb-0.5 uppercase tracking-widest">Total Projects</p>
              <h3 className="text-3xl font-bold text-[#0a2a2a] mb-1.5 tracking-tighter">1951+</h3>
              <div className="flex gap-1 h-8 items-end">
                {[30, 60, 45, 80, 55, 90, 40, 70, 50, 85].map((h, i) => (
                  <div 
                    key={i} 
                    className="flex-1 bg-gray-50 rounded-t-sm transition-all hover:bg-[#dcfce7]" 
                    style={{ height: `${h}%` }} 
                  />
                ))}
              </div>
           </div>
        </motion.div>

        {/* Card 4: Narrow (280px) */}
        <motion.div variants={itemVariants} className="md:col-span-4 h-[280px] bg-[#dcfce7] rounded-xl p-8 flex flex-col justify-center items-center text-center shadow-xl">
           <h3 className="text-4xl font-bold text-[#0a2a2a] mb-2 tracking-tighter">6+</h3>
           <p className="text-[#0a2a2a]/40 text-[13px] font-bold uppercase tracking-widest leading-tight">Years of <br/> Service</p>
        </motion.div>

        {/* Card 5: Edge (350px) - RESTORED DECORATIONS */}
        <motion.div variants={itemVariants} className="md:col-span-5 h-[350px] bg-[#0a2a2a] rounded-2xl p-8 flex flex-col justify-end text-white relative overflow-hidden shadow-2xl">
           {/* Dot Matrix Grid */}
           <div className="absolute inset-0 opacity-[0.03] pointer-events-none" 
                style={{ backgroundImage: 'radial-gradient(#dcfce7 1px, transparent 1px)', backgroundSize: '20px 20px' }} />
           
           {/* Top Wave Restored */}
           <div className="absolute top-0 right-0 w-full opacity-10 rotate-180">
              <svg viewBox="0 0 200 100" className="w-full h-32">
                <path d="M0,80 C50,0 150,100 200,20" fill="none" stroke="#dcfce7" strokeWidth="4" strokeLinecap="round" />
              </svg>
           </div>

           <div className="relative z-10">
              <div className="bg-[#dcfce7] w-9 h-9 rounded-full flex items-center justify-center mb-4 shadow-lg shadow-[#dcfce7]/10">
                <Zap className="w-4 h-4 text-[#0a2a2a]" />
              </div>
              
              <p className="text-lg font-medium leading-[1.3] tracking-tight">
                Achieve Optimal <br/> 
                Efficiency and Boost <br/> 
                Productivity
              </p>
           </div>

           {/* Bottom Wave Restored */}
           <div className="absolute bottom-0 left-0 w-full opacity-20">
              <svg viewBox="0 0 200 100" className="w-full h-24">
                <path d="M0,80 C50,100 150,0 200,80" fill="none" stroke="#dcfce7" strokeWidth="8" strokeLinecap="round" />
              </svg>
           </div>
        </motion.div>
      </motion.div>
    </section>
  );
}
