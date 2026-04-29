
import { motion } from 'framer-motion';
import { CheckCircle2 } from 'lucide-react';

const plans = [
  {
    name: "Starter",
    description: "This package offers the basic features you need to get started.",
    price: "$39",
    features: [
      "Production up to 10,000 units per month",
      "24/7 technical support",
      "Access the production dashboard",
      "Initial setup guide"
    ]
  },
  {
    name: "Enterprise",
    description: "This package provides full access to all premium features.",
    price: "$99",
    features: [
      "Unlimited production units",
      "Dedicated account manager",
      "Tailored manufacturing solutions",
      "Predictive production optimization"
    ]
  }
];

export default function PricingSection() {
  return (
    <section id="pricing" className="bg-[#050f0f] py-48 px-10 relative overflow-hidden">
      {/* Background Glow */}
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-full h-full bg-[radial-gradient(circle_at_50%_-20%,#0a2a2a_0%,transparent_70%)] opacity-50" />

      <div className="max-w-[1200px] mx-auto relative z-10">
        {/* Header */}
        <div className="text-center mb-24">
          <motion.h2 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-4xl md:text-5xl font-bold text-white mb-6 tracking-tight"
          >
            Tailored Plans for Your <br /> Manufacturing Scale
          </motion.h2>
          <motion.p 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
            className="text-white/40 text-lg font-medium"
          >
            Flexible pricing for any business size.
          </motion.p>
        </div>

        {/* Top Grid: Starter & Enterprise */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8">
          {plans.map((plan, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.2 }}
              className="bg-white/[0.03] border border-white/10 rounded-2xl p-12 flex flex-col hover:bg-white/[0.05] transition-all group"
            >
              <h3 className="text-2xl font-bold text-white mb-4">{plan.name}</h3>
              <p className="text-white/40 mb-10 font-medium leading-relaxed max-w-[300px]">
                {plan.description}
              </p>
              
              <div className="flex items-baseline gap-2 mb-10">
                <span className="text-5xl font-bold text-white">{plan.price}</span>
                <span className="text-white/20 font-bold">/ month</span>
              </div>

              <button className="w-full py-4 rounded-full border border-white/10 text-white font-bold mb-12 hover:bg-white hover:text-[#0a2a2a] transition-all">
                Get Started
              </button>

              <div className="space-y-5">
                <p className="text-white/20 uppercase tracking-[0.2em] font-bold text-[10px] mb-6">Features</p>
                {plan.features.map((feature, fIndex) => (
                  <div key={fIndex} className="flex gap-3 items-center">
                    <CheckCircle2 className="w-5 h-5 text-white/20 group-hover:text-white transition-colors" />
                    <span className="text-white/60 font-medium text-sm">{feature}</span>
                  </div>
                ))}
              </div>
            </motion.div>
          ))}
        </div>

        {/* Bottom Card: Professional (Ultra-Subtle 3D Grid) */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ delay: 0.4 }}
          className="relative bg-[#0a2a2a] rounded-2xl p-16 overflow-hidden flex flex-col items-center text-center group min-h-[450px] justify-center"
        >
          {/* Ultra-Subtle 3D Rotated Grid Background */}
          <div className="absolute inset-0 w-full h-full pointer-events-none" style={{ perspective: '1200px' }}>
            <div 
              className="absolute inset-0 w-[300%] h-[300%] -left-[100%] -top-[100%] opacity-[0.05]"
              style={{ 
                backgroundImage: `linear-gradient(to right, #dcfce7 1px, transparent 1px), linear-gradient(to bottom, #dcfce7 1px, transparent 1px)`,
                backgroundSize: '240px 240px',
                transform: 'rotateX(30deg) translateY(-10%)',
                transformOrigin: 'center center',
              }} 
            />
          </div>
          
          <div className="relative z-10">
            <h3 className="text-3xl font-bold text-white mb-6">Professional</h3>
            <p className="text-white/60 text-lg font-medium max-w-[600px] mb-10 leading-relaxed">
              Designed for greater flexibility, this solution offers <br className="hidden md:block" /> 
              advanced tools for custom tailoring to your needs.
            </p>
            
            <button className="bg-[#dcfce7] text-[#0a2a2a] px-12 py-4 rounded-full font-bold text-lg hover:scale-105 transition-transform shadow-xl shadow-[#dcfce7]/10">
              Get Started
            </button>
          </div>
        </motion.div>
      </div>
    </section>
  );
}
