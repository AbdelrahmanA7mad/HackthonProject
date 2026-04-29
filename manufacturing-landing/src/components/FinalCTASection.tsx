
import { motion } from 'framer-motion';

export default function FinalCTASection() {
  return (
    <section className="bg-[#000000] py-32 px-10 relative overflow-hidden flex flex-col items-center justify-center min-h-[700px]">
      
      {/* Ultra-Subtle Full-Coverage 3D Rotated Grid Background */}
      <div className="absolute inset-0 w-full h-full pointer-events-none" style={{ perspective: '1200px' }}>
        <div 
          className="absolute inset-0 w-[300%] h-[300%] -left-[100%] -top-[100%] opacity-[0.05]"
          style={{ 
            backgroundImage: `linear-gradient(to right, #e4e4e7 1px, transparent 1px), linear-gradient(to bottom, #e4e4e7 1px, transparent 1px)`,
            backgroundSize: '240px 240px',
            transform: 'rotateX(30deg) translateY(-10%)',
            transformOrigin: 'center center',
          }} 
        />
      </div>

      {/* Content */}
      <div className="relative z-10 text-center flex flex-col items-center">
        <motion.h2 
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="text-4xl md:text-6xl font-bold text-white mb-8 tracking-tight max-w-[800px] leading-[1.1]"
        >
          From Idea to Production <br className="hidden md:block" /> in Days
        </motion.h2>
        
        <motion.p 
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ delay: 0.1 }}
          className="text-white/40 text-lg md:text-xl font-medium max-w-[650px] mb-12 leading-relaxed"
        >
          Accelerate your production with our technology. Reduce <br className="hidden md:block" />
          downtime and optimize costs. Get a special offer now!
        </motion.p>

        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
          transition={{ delay: 0.2 }}
        >
          <button className="bg-[#e4e4e7] text-[#000000] px-12 py-4 rounded-full font-bold text-lg hover:scale-105 transition-transform shadow-2xl shadow-[#e4e4e7]/20">
            Work With Us
          </button>
        </motion.div>
      </div>

      {/* Subtle Bottom Vignette */}
      <div className="absolute bottom-0 left-0 w-full h-40 bg-gradient-to-t from-black/40 to-transparent" />
    </section>
  );
}
