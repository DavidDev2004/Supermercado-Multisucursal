(() => {
    const modal = document.querySelector('[data-product-modal]');
    if (!modal || !window.bootstrap) return;
    // No se renombran ni eliminan campos: se conserva el token antiforgery.
    modal.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                form.reportValidity();
            }
        });
    });
    bootstrap.Modal.getOrCreateInstance(modal).show();
})();
