// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
  var createForm = document.querySelector('form[action$="/WorkoutLogs/Create"]') || document.querySelector('form[action*="WorkoutLogs/Create"]');
  if (createForm) {
    createForm.addEventListener('submit', function (e) {
      var confirmed = confirm('Save this workout log?');
      if (!confirmed) e.preventDefault();
    });
  }
});
