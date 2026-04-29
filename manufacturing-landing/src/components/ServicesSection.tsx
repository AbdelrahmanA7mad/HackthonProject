
import { motion } from 'framer-motion';
import { 
  Sparkles, 
  Layers, 
  Wrench, 
  ShieldCheck, 
  Box, 
  LineChart, 
  ArrowUpRight 
} from 'lucide-react';

const services = [
  {
    title: "Production and Assembly",
    description: "Details on production processes, assembly, capacity, and product types.",
    icon: Sparkles,
  },
  {
    title: "Custom Manufacturing",
    description: "Custom product creation with design and customization options.",
    icon: Layers,
  },
  {
    title: "Quality Control",
    description: "Procedures and systems in place to ensure high product quality.",
    icon: Wrench,
  },
  {
    title: "Technology and Innovation",
    description: "Details on the latest manufacturing technologies and ongoing innovations.",
    icon: ShieldCheck,
  },
  {
    title: "Packaging and Logistics",
    description: "Packaging and logistics for shipping to customers and distributors.",
    icon: Box,
  },
  {
    title: "Consulting Market Research",
    description: "Services to help companies understand market needs and provide strategic advice.",
    icon: LineChart,
  },
];

const containerVariants = {
  hidden: { opacity: 0 },
  show: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
    },
  },
};

const itemVariants = {
  hidden: { y: 20, opacity: 0 },
  show: { y: 0, opacity: 1 },
};

export default function ServicesSection() {
  return (
    <section id="services" className="bg-[#0a2a2a] py-32 px-10">
      <div className="max-w-[1200px] mx-auto">
        {/* Header */}
        <div className="text-center mb-20">
          <motion.h2 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-4xl md:text-5xl font-bold text-white mb-6 tracking-tight"
          >
            Efficient and Integrated <br className="hidden md:block" /> Manufacturing Services
          </motion.h2>
          <motion.p 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
            className="text-white/40 text-lg max-w-[600px] mx-auto font-medium"
          >
            Simplify operations with our efficient, quality-focused services.
          </motion.p>
        </div>

        {/* Services Grid */}
        <motion.div 
          variants={containerVariants}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true, margin: "-100px" }}
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
        >
          {services.map((service, index) => (
            <motion.div
              key={index}
              variants={itemVariants}
              className="group bg-white/5 border border-white/10 p-8 rounded-2xl hover:bg-white/[0.08] transition-all cursor-pointer relative overflow-hidden"
            >
              {/* Arrow Icon */}
              <div className="absolute top-8 right-8 text-white/20 group-hover:text-white transition-colors">
                <ArrowUpRight className="w-6 h-6" />
              </div>

              {/* Icon Container */}
              <div className="mb-8">
                <service.icon className="w-10 h-10 text-white stroke-[1.5]" />
              </div>

              {/* Content */}
              <div className="mt-auto">
                <h3 className="text-2xl font-bold text-white mb-2 tracking-tight">
                  {service.title}
                </h3>
                <p className="text-white/40 leading-relaxed font-medium">
                  {service.description}
                </p>
              </div>

              {/* Subtle Hover Gradient */}
              <div className="absolute inset-0 bg-gradient-to-br from-white/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none" />
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
