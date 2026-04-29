
import { motion } from 'framer-motion';

const footerLinks = [
  {
    title: "Company",
    links: ["About Us", "Customers", "Newsroom", "Events"]
  },
  {
    title: "Industries",
    links: [
      "Precision Metalforming",
      "Industrial Manufacturing",
      "High Tech & Electronics",
      "Aerospace"
    ]
  },
  {
    title: "Systems",
    links: [
      "Manufacturing Execution",
      "Resource Planning",
      "Management System",
      "Chain Planning"
    ]
  }
];

export default function Footer() {
  return (
    <footer className="bg-[#0a0a0a] pt-24 pb-12 px-10 border-t border-white/5">
      <div className="max-w-[1200px] mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-16 mb-20">
          
          {/* Logo & About */}
          <div className="flex flex-col gap-6">
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 bg-[#000000] rounded-full flex items-center justify-center">
                <div className="w-2.5 h-2.5 bg-[#e4e4e7] rounded-full"></div>
              </div>
              <span className="font-bold text-xl tracking-tight text-white">Prodmast</span>
            </div>
            <p className="text-white/40 text-[15px] leading-relaxed font-medium">
              Our solutions make production <br /> 
              faster and cheaper. Contact us <br /> 
              for more information.
            </p>
          </div>

          {/* Links Columns */}
          {footerLinks.map((column, index) => (
            <div key={index} className="flex flex-col gap-8">
              <h4 className="text-white font-bold text-lg tracking-tight">{column.title}</h4>
              <ul className="flex flex-col gap-4">
                {column.links.map((link, lIndex) => (
                  <li key={lIndex}>
                    <a 
                      href="#" 
                      className="text-white/30 hover:text-white transition-colors text-[15px] font-medium"
                    >
                      {link}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Bottom Bar */}
        <div className="pt-12 border-t border-white/5 flex flex-col md:flex-row justify-between items-center gap-6">
          <p className="text-white/20 text-sm font-medium">
            © 2024 Prodmast. All rights reserved.
          </p>
          <div className="flex gap-8">
            <a href="#" className="text-white/20 hover:text-white transition-colors text-sm font-medium">Privacy Policy</a>
            <a href="#" className="text-white/20 hover:text-white transition-colors text-sm font-medium">Terms of Service</a>
          </div>
        </div>
      </div>
    </footer>
  );
}
