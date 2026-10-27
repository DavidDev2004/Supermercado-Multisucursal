(() => {
    const modal = document.getElementById('salesModal');
    if (!modal || !window.bootstrap) return;
    const readOnly = modal.dataset.readonly === 'true';
    const form = document.getElementById('salesForm');
    const lines = document.getElementById('salesLines');
    const branch = document.getElementById('Input_CodigoSucursal');
    const employee = document.getElementById('Input_CodigoEmpleado');
    const data = JSON.parse(document.getElementById('salesData').textContent);
    const format = value => value.toFixed(4).replace(/(\.\d{2})0+$/, '$1');
    // Entradas a dos decimales; BigInt evita perder precisión al multiplicar.
    const cents = input => {
        if (!/^-?\d+(\.\d{1,2})?$/.test(input.value)) return null;
        const negative = input.value.startsWith('-');
        const [integer, fraction = ''] = input.value.replace('-', '').split('.');
        return BigInt(integer) * (negative ? -100n : 100n) + BigInt(fraction.padEnd(2, '0')) * (negative ? -1n : 1n);
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
        let total = 0n;
        const warnings = [];
        lines.querySelectorAll('.sales-line').forEach((row, index) => {
            [['.line-product','CodigoProducto'],['.line-quantity','Cantidad'],['.line-price','PrecioUnitario'],['.line-discount','Descuento']].forEach(([selector, field]) => {
                row.querySelector(selector).name = `Input.Lineas[${index}].${field}`;
            });
            const quantity = row.querySelector('.line-quantity');
            const price = row.querySelector('.line-price');
            const discount = row.querySelector('.line-discount');
            const q = cents(quantity), p = cents(price), d = cents(discount);
            const net = q !== null && p !== null && d !== null ? q * p - d * 100n : 0n;
            total += net;
            row.querySelector('.line-amount').textContent = amount(net);
            discount.setCustomValidity(q !== null && p !== null && d !== null && d * 100n > q * p ? 'El descuento supera el importe de la línea.' : '');
            const product = Number(row.querySelector('.line-product').value);
            const stock = data.Inventario.find(item => item.CodigoSucursal === Number(branch.value) && item.CodigoProducto === product);
            const cost = row.querySelector('.line-cost');
            cost.textContent = readOnly ? cost.dataset.saved || '—' : stock ? format(stock.CostoPromedio) : 'Sin inventario';
            if (!readOnly && product && (!stock || stock.CostoPromedio <= 0)) warnings.push(`Producto #${product}: falta inventario o su costo promedio es cero.`);
            if (!readOnly && product && stock && Number(quantity.value) > stock.Existencia) warnings.push(`Producto #${product}: existencia actual ${stock.Existencia}. La confirmación volverá a comprobarla.`);
        });
        document.getElementById('salesTotal').textContent = amount(total);
        document.getElementById('salesStockMessage').textContent = [...new Set(warnings)].join(' ');
    };
    if (!readOnly) {
        document.getElementById('addSaleLine').addEventListener('click', () => {
            lines.append(document.getElementById('saleLineTemplate').content.cloneNode(true)); refresh();
        });
        lines.addEventListener('click', event => {
            if (event.target.closest('.remove-line')) { event.target.closest('.sales-line').remove(); refresh(); }
        });
        lines.addEventListener('change', event => {
            if (event.target.matches('.line-product')) {
                const product = data.Productos.find(item => item.Id === Number(event.target.value));
                event.target.closest('.sales-line').querySelector('.line-price').value = product ? product.PrecioVenta.toFixed(2) : '';
            }
            refresh();
        });
        form.addEventListener('input', refresh);
        branch.addEventListener('change', refresh);
        form.addEventListener('submit', refresh);
    }
    refresh();
    bootstrap.Modal.getOrCreateInstance(modal).show();
})();
