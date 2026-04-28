/** @type {import('tailwindcss').Config} */
const withAlpha = (cssVar) => `rgb(var(${cssVar}) / <alpha-value>)`;

module.exports = {
    important: true,
    content: [
        "./Views/**/*.cshtml",
        "./Pages/**/*.cshtml",
        "./wwwroot/js/**/*.js"
    ],
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
            }
        },
    },
    plugins: [],
}
