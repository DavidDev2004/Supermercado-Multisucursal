(() => {
    const modal = document.getElementById('adjustmentsModal');
    if (!modal || !window.bootstrap) return;
    const readOnly = modal.dataset.readonly === 'true';
    const form = document.getElementById('adjustmentsForm');
    const lines = document.getElementById('adjustmentsLines');
    const branch = document.getElementById('Input_CodigoSucursal');
    const employee = document.getElementById('Input_CodigoEmpleado');
    const refresh = () => {
        // Conserva antiforgery: solo se renombran los campos de las líneas.
        [...employee.options].forEach(option => {
            if (!option.value) return;
            option.hidden = !readOnly && option.dataset.branch !== branch.value;
            option.disabled = option.hidden;
        });
        if (!readOnly && employee.selectedOptions[0]?.disabled) employee.value = '';
        const products = new Set();
        lines.querySelectorAll('.adjustment-line').forEach((row, index) => {
            const product = row.querySelector('.line-product');
            const quantity = row.querySelector('.line-quantity');
            product.name = `Input.Lineas[${index}].CodigoProducto`;
            quantity.name = `Input.Lineas[${index}].Cantidad`;
            product.setCustomValidity(product.value && products.has(product.value) ? 'No repitas un producto en el ajuste.' : '');
            if (product.value) products.add(product.value);
            quantity.setCustomValidity(quantity.value !== '' && Number(quantity.value) === 0 ? 'La cantidad debe ser distinta de cero.' : '');
        });
    };
    if (!readOnly) {
        document.getElementById('addAdjustmentLine').addEventListener('click', () => {
            lines.append(document.getElementById('adjustmentLineTemplate').content.cloneNode(true)); refresh();
        });
        lines.addEventListener('click', event => {
            const remove = event.target.closest('.remove-line');
            if (remove) { remove.closest('.adjustment-line').remove(); refresh(); }
        });
        form.addEventListener('input', refresh);
        form.addEventListener('change', refresh);
        form.addEventListener('submit', event => {
            refresh();
            if (!form.checkValidity()) { event.preventDefault(); form.reportValidity(); }
        });
    }
    refresh();
    bootstrap.Modal.getOrCreateInstance(modal).show();
})();
