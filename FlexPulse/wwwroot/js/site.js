// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
  var createForm = document.getElementById('createWorkoutForm');
  if (createForm) {
    var openBtn = document.getElementById('openConfirmBtn');
    var confirmBtn = document.getElementById('confirmSaveBtn');
    var confirmModalEl = document.getElementById('confirmModal');
    var confirmModal = null;
    if (confirmModalEl) {
      confirmModal = new bootstrap.Modal(confirmModalEl);
    }

    if (openBtn && confirmModal) {
      openBtn.addEventListener('click', function (e) {
        confirmModal.show();
      });
    }

    if (confirmBtn) {
      confirmBtn.addEventListener('click', function () {
        // submit the form programmatically
        createForm.submit();
      });
    }
  }
});
