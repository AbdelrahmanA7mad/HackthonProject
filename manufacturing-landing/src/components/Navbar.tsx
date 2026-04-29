import { motion } from 'framer-motion';
import { ArrowUpRight } from 'lucide-react';

const navLinks = [
  { name: "Home", href: "#home" },
  { name: "Services", href: "#services" },
  { name: "Benefits", href: "#benefits" },
  { name: "Pricing", href: "#pricing" },
  { name: "Integrations", href: "#integrations" },
];

export default function Navbar() {
  return (
    <motion.nav
      initial={{ y: -100, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 1.5, ease: [0.16, 1, 0.3, 1] as any, delay: 2.2 }}
      className="fixed top-8 left-1/2 -translate-x-1/2 z-50 w-[calc(100%-80px)] max-w-[1200px]"
    >
      <div className="bg-white/80 backdrop-blur-xl border border-[#000000]/5 px-8 py-3 rounded-full shadow-[0_20px_50px_rgba(0,0,0,0.05)] flex items-center justify-between">
        {/* Logo */}
        <a href="#home" className="flex items-center gap-2.5 group">
          <div className="w-8 h-8 bg-[#000000] rounded-full flex items-center justify-center transition-transform group-hover:scale-110">
            <div className="w-2.5 h-2.5 bg-[#e4e4e7] rounded-full"></div>
          </div>
          <span className="font-bold text-lg tracking-tight text-[#000000]">Prodmast</span>
        </a>

        {/* Links */}
        <div className="hidden md:flex items-center gap-10">
          {navLinks.map((link) => (
            <a
              key={link.name}
              href={link.href}
              className="text-[#000000]/50 hover:text-[#000000] text-[14px] font-bold transition-all hover:scale-105"
            >
              {link.name}
            </a>
          ))}
        </div>

        {/* Action Button */}
        <button className="bg-[#000000] text-white px-7 py-2.5 rounded-full font-bold text-[13px] flex items-center gap-2 hover:scale-105 transition-all shadow-lg shadow-[#000000]/10">
          Work With Us
          <ArrowUpRight className="w-3.5 h-3.5" />
        </button>
      </div>
    </motion.nav>
  );
}
