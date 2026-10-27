(() => {
    const modal = document.getElementById('purchasesModal');
    if (!modal || !window.bootstrap) return;
    const readOnly = modal.dataset.readonly === 'true';
    const form = document.getElementById('purchasesForm');
    const lines = document.getElementById('purchasesLines');
    const supplier = document.getElementById('Input_CodigoProveedor');
    const branch = document.getElementById('Input_CodigoSucursal');
    const employee = document.getElementById('Input_CodigoEmpleado');
    const date = document.getElementById('Input_FechaCompra');
    const delivery = document.getElementById('Input_FechaEntregaProgramada');
    // Cálculo visual exacto con enteros; el servidor valida y calcula nuevamente.
    const cents = input => {
        if (!/^\d+(\.\d{1,2})?$/.test(input.value)) return null;
        const [integer, fraction = ''] = input.value.split('.');
        return BigInt(integer) * 100n + BigInt(fraction.padEnd(2, '0'));
    };
    const amount = value => {
        const sign = value < 0n ? '-' : '';
        const abs = value < 0n ? -value : value;
        return sign + (abs / 10000n) + '.' + (abs % 10000n).toString().padStart(4, '0').replace(/00$/, '');
    };
    const refresh = () => {
        [...employee.options].forEach(option => {
            if (!option.value) return;
            option.hidden = option.dataset.branch !== branch.value;
            option.disabled = option.hidden;
        });
        if (!readOnly && employee.selectedOptions[0]?.disabled) employee.value = '';
        delivery.min = date.value;
        delivery.setCustomValidity(date.value && delivery.value && delivery.value < date.value ? 'La entrega programada no puede ser anterior a la compra.' : '');
        let total = 0n;
        const products = new Set();
        lines.querySelectorAll('.purchases-line').forEach((row, index) => {
            [['.line-product', 'CodigoProducto'], ['.line-quantity', 'Cantidad'], ['.line-price', 'CostoUnitario'], ['.line-discount', 'Descuento'], ['.line-notes', 'Observaciones']].forEach(([selector, field]) => {
                row.querySelector(selector).name = `Input.Lineas[${index}].${field}`;
            });
            const product = row.querySelector('.line-product');
            const selectedProduct = product.value;
            [...product.options].forEach(option => {
                if (!option.value) return;
                const related = supplier.value && option.dataset.suppliers?.split(',').includes(supplier.value);
                option.hidden = !readOnly && !related && option.value !== selectedProduct;
                option.disabled = option.hidden;
            });
            const related = product.selectedOptions[0]?.dataset.suppliers?.split(',').includes(supplier.value);
            product.setCustomValidity(selectedProduct && products.has(selectedProduct)
                ? 'No repitas un producto en la misma compra.'
                : selectedProduct && supplier.value && !related
                    ? 'El producto no está relacionado con el proveedor seleccionado.' : '');
            if (product.value) products.add(product.value);
            const q = cents(row.querySelector('.line-quantity'));
            const p = cents(row.querySelector('.line-price'));
            const discount = row.querySelector('.line-discount');
            const d = cents(discount);
            const net = q !== null && p !== null && d !== null ? q * p - d * 100n : 0n;
            total += net;
            row.querySelector('.line-amount').textContent = amount(net);
            discount.setCustomValidity(q !== null && p !== null && d !== null && d * 100n > q * p ? 'El descuento monetario supera el importe bruto de la línea.' : '');
        });
        document.getElementById('purchasesTotal').textContent = amount(total);
    };
    if (!readOnly) {
        document.getElementById('addPurchaseLine').addEventListener('click', () => {
            lines.append(document.getElementById('purchaseLineTemplate').content.cloneNode(true)); refresh();
        });
        lines.addEventListener('click', event => {
            if (event.target.closest('.remove-line')) { event.target.closest('.purchases-line').remove(); refresh(); }
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
