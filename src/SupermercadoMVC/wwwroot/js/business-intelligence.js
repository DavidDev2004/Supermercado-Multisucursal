(() => {
    const form = document.getElementById('biFilters');
    if (!form) return;
    document.getElementById('biUpdate')?.addEventListener('click', () => form.requestSubmit());
    const applyCrossFilter = (element) => {
            const select = form.elements.namedItem(element.dataset.filter);
            if (!select) return;
            select.value = element.dataset.value;
            form.requestSubmit();
    };
    for (const button of document.querySelectorAll('.bi-cross button, .bi-bars li[data-filter][data-value]')) {
        button.addEventListener('click', () => applyCrossFilter(button));
        if (button.matches('li')) button.addEventListener('keydown', event => {
            if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); applyCrossFilter(button); }
        });
    }
    const seconds = Number(document.getElementById('biRefreshSeconds')?.value || 0);
    if (![30, 60, 120].includes(seconds)) return;
    let navigating = false;
    const timer = window.setInterval(() => {
        if (document.hidden || navigating) return;
        navigating = true;
        form.requestSubmit();
    }, seconds * 1000);
    window.addEventListener('pagehide', () => window.clearInterval(timer), { once: true });
})();
