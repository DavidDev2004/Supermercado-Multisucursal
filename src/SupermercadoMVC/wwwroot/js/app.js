(() => {
    const toggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');
    const closeButton = document.getElementById('menuClose');
    const backdrop = document.getElementById('sidebarBackdrop');
    if (!toggle || !sidebar || !closeButton || !backdrop) return;

    const mobile = window.matchMedia('(max-width: 900px)');
    let returnFocus = null;

    function closeMenu(restoreFocus = true) {
        sidebar.classList.remove('open');
        backdrop.hidden = true;
        document.body.classList.remove('menu-open');
        toggle.setAttribute('aria-expanded', 'false');
        toggle.setAttribute('aria-label', 'Abrir menú');
        if (restoreFocus && mobile.matches && returnFocus) returnFocus.focus();
    }

    function openMenu() {
        returnFocus = document.activeElement;
        sidebar.classList.add('open');
        backdrop.hidden = false;
        document.body.classList.add('menu-open');
        toggle.setAttribute('aria-expanded', 'true');
        toggle.setAttribute('aria-label', 'Cerrar menú');
        closeButton.focus();
    }

    toggle.addEventListener('click', () => sidebar.classList.contains('open') ? closeMenu() : openMenu());
    closeButton.addEventListener('click', () => closeMenu());
    backdrop.addEventListener('click', () => closeMenu());
    sidebar.querySelectorAll('a[href]').forEach(link => link.addEventListener('click', () => {
        if (mobile.matches) closeMenu(false);
    }));
    document.addEventListener('keydown', event => {
        if (!sidebar.classList.contains('open')) return;
        if (event.key === 'Escape') closeMenu();
        if (event.key === 'Tab' && mobile.matches) {
            const focusable = [...sidebar.querySelectorAll('a[href], button:not([disabled])')].filter(el => el.getClientRects().length);
            if (!focusable.length) return;
            const first = focusable[0];
            const last = focusable[focusable.length - 1];
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        }
    });
    mobile.addEventListener('change', () => closeMenu(false));
})();
