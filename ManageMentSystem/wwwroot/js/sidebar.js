/**
 * Sidebar Mobile Menu Handler
 * Handles opening/closing sidebar on mobile devices
 */
(function() {
    'use strict';

    $(document).ready(function() {
        const sidebar = $('#sidebar');
        const overlay = $('#sidebarOverlay');
        const menuBtn = $('#mobileMenuBtn');

        // Exit if elements don't exist
        if (!sidebar.length || !overlay.length || !menuBtn.length) {
            return;
        }

        // فتح/إغلاق القائمة
        menuBtn.on('click', function() {
            if (sidebar.hasClass('active')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        });

        // فتح القائمة
        function openSidebar() {
            sidebar.addClass('active');
            overlay.addClass('active');
            menuBtn.html('<i class="fas fa-times"></i>');
            // Prevent body scroll when sidebar is open
            $('body').css('overflow', 'hidden');
        }

        // إغلاق القائمة
        function closeSidebar() {
            sidebar.removeClass('active');
            overlay.removeClass('active');
            menuBtn.html('<i class="fas fa-bars"></i>');
            // Restore body scroll
            $('body').css('overflow', '');
        }

        // إغلاق القائمة عند الضغط على الخلفية
        overlay.on('click', function() {
            closeSidebar();
        });

        // عدم إغلاق القائمة عند الضغط على dropdown toggle
        $('.sidebar-light .dropdown-toggle').on('click', function(e) {
            e.stopPropagation();
            // إغلاق جميع القوائم المنسدلة الأخرى
            $('.sidebar-light .dropdown-menu').not($(this).next()).removeClass('show');
        });

        // إغلاق القائمة فقط عند اختيار عنصر من القائمة المنسدلة (ليس dropdown-toggle)
        $('.sidebar-light .dropdown-item').on('click', function() {
            if ($(window).width() <= 768) {
                closeSidebar();
            }
        });

        // إغلاق القائمة عند اختيار رابط عادي (ليس dropdown)
        $('.sidebar-light .nav-link').not('.dropdown-toggle').on('click', function() {
            if ($(window).width() <= 768) {
                closeSidebar();
            }
        });

        // إغلاق القائمة عند تغيير حجم الشاشة
        $(window).on('resize', function() {
            if ($(window).width() > 768) {
                closeSidebar();
                // إزالة كل القوائم المفتوحة
                $('.sidebar-light .dropdown-menu').removeClass('show');
            }
        });

        // تحديث الـ navbar عند حذف فئة (handled by server-side TempData)
        // This will be handled inline in Layout.cshtml if needed
    });

    // ================================================================
    // Sidebar Theme Toggle Handler
    // ================================================================
    
    function setupSidebarThemeToggle() {
        const sidebarThemeToggle = document.getElementById('sidebarThemeToggle');
        const sidebarThemeIcon = document.getElementById('sidebarThemeIcon');
        const sidebarThemeText = document.getElementById('sidebarThemeText');

        if (!sidebarThemeToggle) {
            return;
        }

        function updateSidebarThemeIcon(theme) {
            if (sidebarThemeIcon) {
                sidebarThemeIcon.textContent = theme === 'dark' ? '☀️' : '🌙';
            }
            if (sidebarThemeText) {
                sidebarThemeText.textContent = theme === 'dark' ? 'الوضع النهاري' : 'الوضع الليلي';
            }
        }

        // Update icon on initial load
        const initialTheme = document.documentElement.getAttribute('data-theme') || 'light';
        updateSidebarThemeIcon(initialTheme);

        sidebarThemeToggle.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

            console.log('Sidebar: Toggling theme from', currentTheme, 'to', newTheme);
            
            // Apply theme change
            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateSidebarThemeIcon(newTheme);
            
            console.log('Sidebar: Theme changed successfully to:', newTheme);

            // Dispatch custom event for other listeners (like navbar-unified.js)
            window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme: newTheme } }));
        });

        // Listen for theme changes from other sources (like navbar)
        window.addEventListener('themeChanged', (e) => {
            const newTheme = e.detail.theme;
            updateSidebarThemeIcon(newTheme);
        });
    }

    // Initialize sidebar theme toggle
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', setupSidebarThemeToggle);
    } else {
        setupSidebarThemeToggle();
    }
})();

