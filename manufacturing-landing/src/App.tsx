import Loader from '@/components/Loader';
import Navbar from '@/components/Navbar';
import HeroSection from '@/components/HeroSection';
import ServicesSection from '@/components/ServicesSection';
import BenefitsSection from '@/components/BenefitsSection';
import PricingSection from '@/components/PricingSection';
import IntegrationsSection from '@/components/IntegrationsSection';
import FinalCTASection from '@/components/FinalCTASection';
import Footer from '@/components/Footer';

function App() {
  return (
    <main className="min-h-screen bg-[#fafafa]">
      <Loader />
      <Navbar />
      <HeroSection />
      <ServicesSection />
      <BenefitsSection />
      <PricingSection />
      <IntegrationsSection />
      <FinalCTASection />
      <Footer />
    </main>
  );
}

export default App;
