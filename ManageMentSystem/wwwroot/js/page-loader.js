/**
 * Page Loader (Top Progress Bar)
 * Shows a loading indicator at the top of the page during navigation
 */
(function() {
    'use strict';

    const bar = document.getElementById('pageLoader');
    
    if (!bar) {
        return;
    }

    let progress = 0;
    let timer = null;

    function start() {
        bar.classList.add('show');
        progress = 0;
        bar.style.width = '0%';
        clearInterval(timer);
        
        timer = setInterval(() => {
            // Ease to 80% while loading
            progress = Math.min(80, progress + Math.max(1, (80 - progress) * 0.1));
            bar.style.width = progress + '%';
        }, 120);
    }

    function finish() {
        clearInterval(timer);
        bar.style.width = '100%';
        setTimeout(() => {
            bar.classList.remove('show');
            bar.style.opacity = '0';
        }, 120);
        setTimeout(() => {
            bar.style.width = '0%';
            bar.style.opacity = '1';
        }, 320);
    }

    function shouldIgnoreLink(a) {
        const href = a.getAttribute('href') || '';
        return (
            a.hasAttribute('download') ||
            a.target === '_blank' ||
            href.startsWith('#') ||
            href.startsWith('javascript:') ||
            a.getAttribute('data-no-loader') === 'true'
        );
    }

    // Handle link clicks
    document.addEventListener('click', function(e) {
        const a = e.target.closest('a[href]');
        if (!a) return;
        if (shouldIgnoreLink(a)) return;
        if (e.ctrlKey || e.metaKey || e.shiftKey || e.button === 1) return;
        start();
    }, true);

    // Finish loading on page show
    window.addEventListener('pageshow', finish);
    
    // Start loading on before unload
    window.addEventListener('beforeunload', start);
})();

