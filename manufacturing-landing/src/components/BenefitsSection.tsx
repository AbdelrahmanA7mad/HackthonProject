import { motion } from 'framer-motion';
import { CheckCircle2 } from 'lucide-react';

const benefits = [
  {
    title: "Boosting Quality with Tech",
    description: "With advanced technology, we help you achieve top product quality. Discover how we can enhance your standards.",
  },
  {
    title: "Optimization Production Process",
    description: "Boost factory efficiency and productivity with our innovative solutions. See how the latest technology can maximize your output.",
  },
  {
    title: "AI-Driven Production",
    description: "Leverage the power of AI to transform your manufacturing processes, achieving faster and more effective results.",
  },
];

export default function BenefitsSection() {
  return (
    <section id="benefits" className="bg-white py-32 px-10">
      <div className="max-w-[1200px] mx-auto grid grid-cols-1 lg:grid-cols-2 gap-20 items-stretch">
        
        {/* Left Column: Image with Decorative Frame */}
        <div className="relative p-8 lg:p-12 group/frame h-full">
          {/* Background Decorative Elements - Technical Style (No solid bg) */}
          <div className="absolute inset-4 border border-[#000000]/10 rounded-[24px] pointer-events-none" />
          
          {/* Corner accents */}
          <div className="absolute top-4 left-4 w-8 h-8 border-t-2 border-l-2 border-[#000000]/20 rounded-tl-xl" />
          <div className="absolute bottom-4 right-4 w-8 h-8 border-b-2 border-r-2 border-[#000000]/20 rounded-br-xl" />

          <motion.div 
            initial={{ opacity: 0, x: -40 }}
            whileInView={{ opacity: 1, x: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 1, ease: [0.16, 1, 0.3, 1] as any }}
            className="relative h-full min-h-[500px] rounded-2xl overflow-hidden shadow-2xl group z-10"
          >
            <img 
              src="/img/engineer.png" 
              alt="Manufacturing Engineer" 
              className="absolute inset-0 w-full h-full object-cover transition-transform duration-700 group-hover:scale-105"
            />
            {/* Overlay for depth */}
            <div className="absolute inset-0 bg-gradient-to-t from-[#000000]/20 to-transparent" />
          </motion.div>
        </div>

        {/* Right Column: Content */}
        <div className="flex flex-col">
          <motion.h2 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-4xl md:text-5xl font-bold text-[#000000] mb-6 leading-[1.1] tracking-tight"
          >
            Key Benefits of Our System <br /> for Your Business Efficiency
          </motion.h2>
          
          <motion.p 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
            className="text-[#000000]/60 text-lg mb-12 font-medium max-w-[500px]"
          >
            Our systems boost productivity, cut costs, and drive business growth.
          </motion.p>

          <div className="flex flex-col gap-8">
            {benefits.map((benefit, index) => (
              <motion.div 
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: 0.2 + index * 0.1 }}
                className="flex gap-5 items-start group"
              >
                <div className="mt-1 bg-[#e4e4e7] p-1.5 rounded-full text-[#000000] group-hover:scale-110 transition-transform">
                  <CheckCircle2 className="w-5 h-5 fill-[#000000] text-white" />
                </div>
                <div>
                  <h3 className="text-xl font-bold text-[#000000] mb-2">{benefit.title}</h3>
                  <p className="text-[#000000]/60 leading-relaxed font-medium max-w-[450px]">
                    {benefit.description}
                  </p>
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
