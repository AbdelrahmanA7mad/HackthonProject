
import { motion } from 'framer-motion';
import { 
  Box, 
  Cpu, 
  Cloud, 
  Database, 
  Terminal, 
  Activity, 
  Layers, 
  Zap,
  Globe
} from 'lucide-react';

const integrationIcons = [
  { Icon: Box, color: "#0a2a2a", top: "10%", left: "50%" },
  { Icon: Cpu, color: "#0a2a2a", top: "30%", left: "85%" },
  { Icon: Cloud, color: "#0a2a2a", top: "70%", left: "80%" },
  { Icon: Database, color: "#0a2a2a", top: "85%", left: "45%" },
  { Icon: Terminal, color: "#0a2a2a", top: "70%", left: "15%" },
  { Icon: Activity, color: "#0a2a2a", top: "30%", left: "10%" },
  { Icon: Layers, color: "#0a2a2a", top: "45%", left: "55%" },
  { Icon: Zap, color: "#0a2a2a", top: "55%", left: "35%" },
];

export default function IntegrationsSection() {
  return (
    <section id="integrations" className="bg-white py-32 px-10">
      <div className="max-w-[1200px] mx-auto grid grid-cols-1 lg:grid-cols-2 gap-20 items-center">
        
        {/* Left Column: Content */}
        <div className="flex flex-col order-2 lg:order-1">
          <motion.h2 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-4xl md:text-5xl font-bold text-[#0a2a2a] mb-8 leading-[1.1] tracking-tight"
          >
            Empowering Top Companies <br /> with Seamless Integrations
          </motion.h2>
          
          <motion.p 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
            className="text-[#0a2a2a]/60 text-lg mb-12 font-medium max-w-[550px] leading-relaxed"
          >
            Experience seamless connections with our innovative solutions, designed 
            to effortlessly integrate with your existing systems, enhance productivity, 
            and drive your business towards greater success.
          </motion.p>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.2 }}
          >
            <button className="bg-[#dcfce7] text-[#0a2a2a] px-10 py-4 rounded-full font-bold text-lg hover:scale-105 transition-transform shadow-xl shadow-[#dcfce7]/10">
              Work With Us
            </button>
          </motion.div>
        </div>

        {/* Right Column: Orbital Graphic */}
        <motion.div 
          initial={{ opacity: 0, scale: 0.9 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
          transition={{ duration: 1 }}
          className="relative h-[500px] bg-[#f0fdf4] rounded-2xl flex items-center justify-center overflow-hidden order-1 lg:order-2"
        >
          {/* Concentric Rings */}
          <div className="absolute w-[380px] h-[380px] border border-[#0a2a2a]/5 rounded-full" />
          <div className="absolute w-[240px] h-[240px] border border-[#0a2a2a]/5 rounded-full" />
          <div className="absolute w-[100px] h-[100px] border border-[#0a2a2a]/5 rounded-full" />

          {/* Floating Icons */}
          {integrationIcons.map((item, index) => (
            <motion.div
              key={index}
              animate={{ 
                y: [0, -15, 0],
                x: [0, 10, 0]
              }}
              transition={{ 
                duration: 4 + index, 
                repeat: Infinity, 
                ease: "easeInOut",
                delay: index * 0.5
              }}
              style={{ 
                position: 'absolute',
                top: item.top,
                left: item.left,
              }}
              className="bg-white p-3.5 rounded-2xl shadow-[0_10px_30px_rgba(0,0,0,0.05)] border border-gray-50 flex items-center justify-center group hover:scale-110 transition-transform cursor-pointer"
            >
              <item.Icon className="w-6 h-6 stroke-[1.5]" style={{ color: item.color }} />
            </motion.div>
          ))}

          {/* Center Icon */}
          <div className="relative bg-[#0a2a2a] p-5 rounded-xl shadow-2xl z-10">
            <Globe className="w-10 h-10 text-[#dcfce7]" />
          </div>

          {/* Subtle Radial Glow */}
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_center,transparent_0%,white_100%)] opacity-40" />
        </motion.div>
      </div>
    </section>
  );
}
