document.addEventListener('DOMContentLoaded', function () {
    console.log('GestionPistasWeb - App iniciada');

    var alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});
