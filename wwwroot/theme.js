// Theme Management
(function () {
    // Initialize theme from localStorage or default to dark
    function initTheme() {
        const savedTheme = localStorage.getItem('theme') || 'dark';
        applyTheme(savedTheme);
    }

    // Apply theme to document
    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        // Update color-scheme for better browser integration
        document.documentElement.style.colorScheme = theme;
    }

    // Toggle theme
    window.toggleTheme = function () {
        const currentTheme = document.documentElement.getAttribute('data-theme') || 'dark';
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

        applyTheme(newTheme);
        localStorage.setItem('theme', newTheme);

        return newTheme;
    };

    // Get current theme
    window.getCurrentTheme = function () {
        return document.documentElement.getAttribute('data-theme') || 'dark';
    };

    // Initialize on load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTheme);
    } else {
        initTheme();
    }
})();
