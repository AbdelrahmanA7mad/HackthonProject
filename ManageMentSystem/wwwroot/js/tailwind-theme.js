const withAlpha = (cssVar) => `rgb(var(${cssVar}) / <alpha-value>)`;

tailwind.config = {
    important: true,
    theme: {
        extend: {
            colors: {
                primary: withAlpha('--zen-theme-primary-rgb'),
                primaryHover: withAlpha('--zen-theme-primary-hover-rgb'),
                primaryLight: withAlpha('--zen-theme-primary-light-rgb'),
                bgBase: withAlpha('--zen-theme-bg-base-rgb'),
                bgSubtle: withAlpha('--zen-theme-bg-subtle-rgb'),
                borderSubtle: withAlpha('--zen-theme-border-subtle-rgb'),
                textMuted: withAlpha('--zen-theme-text-muted-rgb'),
                accent: withAlpha('--zen-theme-accent-rgb'),
                accentLight: withAlpha('--zen-theme-accent-light-rgb'),
                accentRed: withAlpha('--zen-theme-accent-red-rgb'),
                accentGreen: withAlpha('--zen-theme-accent-green-rgb'),
                accentAmber: withAlpha('--zen-theme-accent-amber-rgb'),
            },
            fontFamily: {
                sans: ['Cairo', 'sans-serif'],
            },
        }
    }
};

const themeTokenMap = {
    primary: { cssVar: '--zen-theme-primary', fallback: '#0f1419' },
    primaryHover: { cssVar: '--zen-theme-primary-hover', fallback: '#1a1f24' },
    primaryLight: { cssVar: '--zen-theme-primary-light', fallback: '#f0f1f2' },
    bgBase: { cssVar: '--zen-theme-bg-base', fallback: '#ffffff' },
    bgSubtle: { cssVar: '--zen-theme-bg-subtle', fallback: '#f8f9fa' },
    border: { cssVar: '--zen-theme-border-subtle', fallback: '#e5e7eb' },
    borderSubtle: { cssVar: '--zen-theme-border-subtle', fallback: '#e5e7eb' },
    textMuted: { cssVar: '--zen-theme-text-muted', fallback: '#6b7280' },
    accent: { cssVar: '--zen-theme-accent', fallback: '#315f8f' },
    accentLight: { cssVar: '--zen-theme-accent-light', fallback: '#d1e3f8' },
    accentRed: { cssVar: '--zen-theme-accent-red', fallback: '#e11d48' },
    accentGreen: { cssVar: '--zen-theme-accent-green', fallback: '#059669' },
    accentAmber: { cssVar: '--zen-theme-accent-amber', fallback: '#d97706' },
};

const readThemeToken = (cssVar, fallback) => {
    const value = getComputedStyle(document.documentElement).getPropertyValue(cssVar).trim();
    return value || fallback;
};

window.THEME = {};

Object.entries(themeTokenMap).forEach(([key, token]) => {
    Object.defineProperty(window.THEME, key, {
        enumerable: true,
        get() {
            return readThemeToken(token.cssVar, token.fallback);
        }
    });

    Object.defineProperty(window, key, {
        enumerable: true,
        configurable: true,
        get() {
            return readThemeToken(token.cssVar, token.fallback);
        },
        set(value) {
            document.documentElement.style.setProperty(token.cssVar, value);
        }
    });
});
