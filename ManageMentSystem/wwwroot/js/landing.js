const loader = document.getElementById('loader');
const menuBtn = document.getElementById('menuBtn');
const mobileMenu = document.getElementById('mobileMenu');
const metricNumbers = document.querySelectorAll('.metric-number');
const liveValues = document.querySelectorAll('.live-value');

setTimeout(() => {
    if (!loader) return;
    loader.classList.add('opacity-0', 'pointer-events-none');
    setTimeout(() => loader.remove(), 700);
}, 2800);

if (menuBtn && mobileMenu) {
    menuBtn.addEventListener('click', () => {
        mobileMenu.classList.toggle('hidden');
    });
}

document.querySelectorAll('#mobileMenu a').forEach((link) => {
    link.addEventListener('click', () => mobileMenu?.classList.add('hidden'));
});

const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add('is-visible');
            if (entry.target.classList.contains('metric-card')) {
                const metric = entry.target.querySelector('.metric-number');
                if (metric && !metric.dataset.animated) {
                    animateCounter(metric);
                    metric.dataset.animated = 'true';
                }
            }
            observer.unobserve(entry.target);
        }
    });
}, { threshold: 0.15 });

document.querySelectorAll('.reveal').forEach((el) => observer.observe(el));
document.querySelectorAll('.metric-card').forEach((el) => observer.observe(el));

function animateCounter(element) {
    const target = Number(element.dataset.target || 0);
    const hasDecimal = String(element.dataset.target || '').includes('.');
    const duration = 1300;
    const start = performance.now();

    function update(now) {
        const progress = Math.min((now - start) / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        const value = target * eased;

        element.textContent = hasDecimal ? value.toFixed(1) : Math.floor(value).toString();

        if (progress < 1) {
            requestAnimationFrame(update);
        }
    }

    requestAnimationFrame(update);
}

metricNumbers.forEach((metric) => {
    metric.textContent = '0';
});

function startLiveTicker() {
    if (!liveValues.length) return;
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    liveValues.forEach((value) => {
        let current = Number(value.textContent) || Number(value.dataset.liveMin || 0);

        const tick = () => {
            const min = Number(value.dataset.liveMin || current);
            const max = Number(value.dataset.liveMax || current);
            const next = Math.floor(Math.random() * (max - min + 1)) + min;
            const duration = 700;
            const start = performance.now();
            const from = current;

            function animate(now) {
                const progress = Math.min((now - start) / duration, 1);
                const eased = 1 - Math.pow(1 - progress, 3);
                const frameValue = Math.round(from + (next - from) * eased);
                value.textContent = frameValue.toString();

                if (progress < 1) {
                    requestAnimationFrame(animate);
                } else {
                    current = next;
                }
            }

            requestAnimationFrame(animate);
        };

        tick();
        setInterval(tick, 2200 + Math.random() * 1000);
    });
}

startLiveTicker();
