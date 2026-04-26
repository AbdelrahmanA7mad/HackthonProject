/**
 * ========================================
 * Zenith ERP — Design System (Monochrome)
 * ========================================
 * Single source of truth for all colors used across the application.
 * Mirrors the nexa-erp React project color palette.
 *
 * PALETTE:
 *   primary      — #0f1419  (deep black — buttons, active items, headings)
 *   primaryHover — #1a1f24  (slightly lighter black for hover states)
 *   bgBase       — #ffffff  (page background)
 *   bgSubtle     — #f8f9fa  (card/panel backgrounds, badges)
 *   borderSubtle — #e5e7eb  (borders, dividers)
 *   textMuted    — #6b7280  (secondary text / gray-500)
 *   accentRed    — #e11d48  (destructive actions only)
 *   accentGreen  — #059669  (success states only)
 *   accentAmber  — #d97706  (warning states only — use sparingly)
 */

// ── Tailwind runtime config (used by the CDN build) ──────────────────────────
tailwind.config = {
    theme: {
        extend: {
            colors: {
                primary:       '#0f1419',
                primaryHover:  '#1a1f24',
                bgBase:        '#ffffff',
                bgSubtle:      '#f8f9fa',
                borderSubtle:  '#e5e7eb',
                textMuted:     '#6b7280',
                accentRed:     '#e11d48',
                accentGreen:   '#059669',
                accentAmber:   '#d97706',
            },
            fontFamily: {
                sans: ['Cairo', 'sans-serif'],
            },
        }
    }
};

// ── JS color tokens (for use in inline scripts / JS components) ───────────────
window.THEME = {
    primary:       '#0f1419',
    primaryHover:  '#1a1f24',
    bgBase:        '#ffffff',
    bgSubtle:      '#f8f9fa',
    border:        '#e5e7eb',
    textMuted:     '#6b7280',
    accentRed:     '#e11d48',
    accentGreen:   '#059669',
    accentAmber:   '#d97706',
};
