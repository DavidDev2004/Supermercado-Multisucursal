document.addEventListener('DOMContentLoaded', () => {
    const modal = document.getElementById('paymentsModal');
    if (modal) bootstrap.Modal.getOrCreateInstance(modal).show();
    const form = document.getElementById('paymentForm');
    if (!form) return;
    const amount = form.querySelector('[name="Input.Monto"]');
    const fee = form.querySelector('[name="Input.Comision"]');
    // Comparación exacta en centavos; el saldo autoritativo se calcula con DECIMAL en SQL Server.
    const cents = text => {
        if (!/^\d+(\.\d{1,2})?$/.test(text)) return null;
        const [whole, fraction = ''] = text.split('.');
        return BigInt(whole) * 100n + BigInt(fraction.padEnd(2, '0'));
    };
    const validate = () => {
        fee.setCustomValidity('');
        const a = cents(amount.value), f = cents(fee.value);
        if (a !== null && f !== null && f > a) fee.setCustomValidity('La comisión no puede superar el monto del abono.');
    };
    amount.addEventListener('input', validate);
    fee.addEventListener('input', validate);
    form.addEventListener('submit', validate);
    validate();
});
