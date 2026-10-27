document.addEventListener('DOMContentLoaded', () => {
    const page = document.querySelector('.inicio-dashboard');
    if (!page) return;
    const status = page.querySelector('.inicio-loading');
    const loading = () => { status.hidden = false; page.setAttribute('aria-busy', 'true'); };
    page.querySelector('.inicio-search-form')?.addEventListener('submit', loading);
    page.querySelectorAll('a.inicio-link').forEach(link => link.addEventListener('click', event => {
        if (!event.ctrlKey && !event.metaKey && !event.shiftKey && event.button === 0) loading();
    }));
    window.addEventListener('pageshow', () => { status.hidden = true; page.removeAttribute('aria-busy'); });
});
