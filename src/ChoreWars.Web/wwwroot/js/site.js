// Dark Mode Management for ChoreWars
(function () {
    'use strict';

    const STORAGE_KEY = 'chorewars-theme';
    const THEME_DARK = 'dark';
    const THEME_LIGHT = 'light';
    const DEFAULT_THEME = THEME_DARK; // Default to dark mode

    // Get current theme from localStorage or default
    function getStoredTheme() {
        try {
            return localStorage.getItem(STORAGE_KEY);
        } catch (e) {
            // localStorage unavailable, return null to use default
            return null;
        }
    }

    // Get preferred theme (stored or default)
    function getPreferredTheme() {
        const storedTheme = getStoredTheme();
        if (storedTheme) {
            return storedTheme;
        }
        return DEFAULT_THEME;
    }

    // Set theme on document
    function setTheme(theme) {
        if (theme === THEME_DARK) {
            document.documentElement.setAttribute('data-bs-theme', THEME_DARK);
        } else {
            document.documentElement.removeAttribute('data-bs-theme');
        }
    }

    // Save theme to localStorage
    function saveTheme(theme) {
        try {
            localStorage.setItem(STORAGE_KEY, theme);
        } catch (e) {
            // Silent fail - theme won't persist but toggle still works
            console.warn('Could not save theme preference:', e);
        }
    }

    // Update toggle button icon and aria-label
    function updateToggleButton(theme) {
        const toggleBtn = document.getElementById('theme-toggle');
        if (!toggleBtn) return;

        if (theme === THEME_DARK) {
            // In dark mode, show sun icon (click to go light)
            toggleBtn.innerHTML = '☀️';
            toggleBtn.setAttribute('aria-label', 'Switch to light mode');
            toggleBtn.setAttribute('title', 'Switch to light mode');
        } else {
            // In light mode, show moon icon (click to go dark)
            toggleBtn.innerHTML = '🌙';
            toggleBtn.setAttribute('aria-label', 'Switch to dark mode');
            toggleBtn.setAttribute('title', 'Switch to dark mode');
        }
    }

    // Toggle between themes
    function toggleTheme() {
        const currentTheme = getPreferredTheme();
        const newTheme = currentTheme === THEME_DARK ? THEME_LIGHT : THEME_DARK;

        setTheme(newTheme);
        saveTheme(newTheme);
        updateToggleButton(newTheme);
    }

    // Initialize theme on page load
    function initTheme() {
        const theme = getPreferredTheme();
        setTheme(theme);
        updateToggleButton(theme);
    }

    // Expose functions globally for inline script
    window.ChoreWarsTheme = {
        init: initTheme,
        toggle: toggleTheme,
        getPreferred: getPreferredTheme,
        set: setTheme
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTheme);
    } else {
        initTheme();
    }
})();
