// Sistema de Ventas - Scripts Globales

// Formateador de moneda en Bolivianos (Bs)
function formatCurrency(amount) {
    var val = parseFloat(amount) || 0;
    return 'Bs ' + val.toFixed(2);
}

// Auto-cerrar alertas después de 4 segundos
document.addEventListener('DOMContentLoaded', function () {
    var alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 4500);
    });
});
