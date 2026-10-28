(() => {
    const modalElement = document.getElementById('recordModal');
    const form = document.getElementById('recordForm');
    if (!modalElement || !form || !window.bootstrap) return;

    const title = document.getElementById('recordModalTitle');
    const submit = document.getElementById('recordSubmit');
    const idInput = form.querySelector('input[name="Id"]');
    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
    const setMode = mode => {
        const isEdit = mode === 'edit';
        const singular = modalElement.dataset.singularTitle || 'registro';
        title.textContent = `${isEdit ? 'Editar' : 'Agregar'} ${singular}`;
        submit.textContent = isEdit ? 'Guardar cambios' : 'Guardar registro';
    };

    modalElement.addEventListener('show.bs.modal', event => {
        const trigger = event.relatedTarget;
        if (!trigger) return;
        const mode = trigger.dataset.mode || 'create';
        const fields = [...form.querySelectorAll('[data-field]')];
        const record = mode === 'edit'
            ? JSON.parse(trigger.closest('tr')?.dataset.record || '{}')
            : {};

        setMode(mode);
        if (mode === 'edit') {
            const id = record[modalElement.dataset.idColumn];
            const editTemplate = modalElement.dataset.editUrlTemplate;
            form.action = editTemplate.replace(/\/0(?:\?.*)?$/, `/${encodeURIComponent(id)}`);
            idInput.value = id ?? '';
        } else {
            form.action = modalElement.dataset.createUrl;
            idInput.value = '';
        }

        fields.forEach(input => {
            const value = mode === 'edit' ? record[input.dataset.field] : input.dataset.default || '';
            const normalized = value == null ? '' : String(value);
            input.value = input.type === 'date' ? normalized.slice(0, 10) : normalized;
        });

        // Keep __RequestVerificationToken intact; clearing it makes every catalog POST fail with HTTP 400.
    });

    const state = document.getElementById('modalState');
    if (state?.dataset.open) {
        setMode(state.dataset.open);
        if (state.dataset.open === 'edit' && idInput.value) {
            form.action = modalElement.dataset.editUrlTemplate.replace(/\/0(?:\?.*)?$/, `/${encodeURIComponent(idInput.value)}`);
        }
        modal.show();
    }
})();
