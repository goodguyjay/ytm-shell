(function(css) {
    function injectStyle() {
        const existing = document.querySelector('style[data-ytmshell]');
        if (existing) 
            existing.remove();
        const s = document.createElement('style');
        s.setAttribute('data-ytmshell', '');
        s.textContent = css;
        document.head.appendChild(s);
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', injectStyle);
    } else {
        injectStyle();
    }
})(`{CSS}`);