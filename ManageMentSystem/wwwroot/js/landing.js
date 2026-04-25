// ================================================================
// Navbar functionality is now handled by navbar-unified.js
// ================================================================

// ================================================================
// Smooth Scroll Handler for Anchor Links
// ================================================================

function animateCounter(element, target) {
  let current = 0; const increment = target / 100; const duration = 2000; const stepTime = duration / 100;
  const timer = setInterval(() => {
    current += increment;
    if (current >= target) { current = target; clearInterval(timer); }
    element.textContent = Math.floor(current).toLocaleString('ar-SA');
  }, stepTime);
}

const observerOptions = { threshold: 0.3, rootMargin: '0px' };
const statsObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      const statNumbers = entry.target.querySelectorAll('.stat-number');
      statNumbers.forEach(stat => { const target = parseInt(stat.getAttribute('data-count')); animateCounter(stat, target); });
      statsObserver.unobserve(entry.target);
    }
  });
}, observerOptions);
const heroStats = document.querySelector('.hero-stats'); if (heroStats) { statsObserver.observe(heroStats); }

const animationObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => { if (entry.isIntersecting) { entry.target.classList.add('aos-animate'); animationObserver.unobserve(entry.target); } });
}, observerOptions);
document.querySelectorAll('[data-aos]').forEach(el => animationObserver.observe(el));

const buttons = document.querySelectorAll('.btn');
buttons.forEach(button => {
  button.addEventListener('mouseenter', function() {
    const ripple = document.createElement('span');
    ripple.classList.add('ripple');
    this.appendChild(ripple);
    setTimeout(() => ripple.remove(), 600);
  });
});

// Handle contact form if exists
const contactForm = document.querySelector('.contact-form');
if (contactForm) {
  contactForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const submitButton = contactForm.querySelector('button[type="submit"]');
    const originalText = submitButton.textContent;
    submitButton.textContent = 'جاري الإرسال...'; 
    submitButton.disabled = true;
    setTimeout(() => {
      submitButton.textContent = 'تم الإرسال بنجاح!';
      submitButton.style.background = '#10b981';
      setTimeout(() => { 
        submitButton.textContent = originalText; 
        submitButton.style.background = ''; 
        submitButton.disabled = false; 
        contactForm.reset(); 
      }, 2000);
    }, 1500);
  });
}

const style = document.createElement('style');
style.textContent = `
@keyframes ripple {
  to {
    transform: scale(4);
    opacity: 0;
  }
}

.ripple {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.6);
  width: 20px;
  height: 20px;
  animation: ripple 0.6s ease-out;
  pointer-events: none;
}

.btn {
  position: relative;
  overflow: hidden;
}
`;
document.head.appendChild(style);

// Public pages: top loading bar on navigation
(function(){
  const bar = document.createElement('div');
  bar.id = 'pageLoader';
  bar.className = 'page-loading-bar';
  document.body.appendChild(bar);

  let progress = 0, timer = null;
  function start(){
    bar.classList.add('show');
    progress = 0; bar.style.width = '0%';
    clearInterval(timer);
    timer = setInterval(()=>{ progress = Math.min(80, progress + Math.max(1,(80-progress)*0.1)); bar.style.width = progress+'%'; },120);
  }
  function finish(){
    clearInterval(timer);
    bar.style.width = '100%';
    setTimeout(()=>{ bar.classList.remove('show'); bar.style.opacity='0'; },120);
    setTimeout(()=>{ bar.style.width='0%'; bar.style.opacity='1'; },320);
  }
  function ignore(a){
    const href = a.getAttribute('href') || '';
    return a.hasAttribute('download') || a.target === '_blank' || href.startsWith('#') || href.startsWith('javascript:') || a.getAttribute('data-no-loader') === 'true';
  }
  document.addEventListener('click', (e)=>{
    const a = e.target.closest('a[href]');
    if(!a) return; if(ignore(a)) return; if(e.ctrlKey||e.metaKey||e.shiftKey||e.button===1) return; start();
  }, true);
  window.addEventListener('pageshow', finish);
  window.addEventListener('beforeunload', start);
})();