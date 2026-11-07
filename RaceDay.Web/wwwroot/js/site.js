// RaceDay UI helpers (presentation only).
(function () {
    // Confirm before destructive actions: <form data-confirm="Delete this event?">
    document.addEventListener('submit', function (e) {
        var msg = e.target && e.target.getAttribute && e.target.getAttribute('data-confirm');
        if (msg && !window.confirm(msg)) e.preventDefault();
    });

    // Disable submit buttons after the first click to prevent double submissions.
    document.addEventListener('submit', function (e) {
        if (e.defaultPrevented) return;
        var btn = e.target.querySelector && e.target.querySelector('button[type=submit][data-busy]');
        if (btn) { btn.disabled = true; btn.dataset.label = btn.textContent; btn.textContent = btn.getAttribute('data-busy'); }
    });

    // Live preview for image inputs: <input type="file" data-preview="#imgId">
    document.querySelectorAll('input[type=file][data-preview]').forEach(function (input) {
        input.addEventListener('change', function () {
            var target = document.querySelector(input.getAttribute('data-preview'));
            var file = input.files && input.files[0];
            if (!target || !file) return;
            if (file.size > 5 * 1024 * 1024) { alert('Please choose an image of 5 MB or less.'); input.value = ''; return; }
            target.src = URL.createObjectURL(file);
            target.classList.remove('d-none');
            var hide = input.getAttribute('data-preview-hide');
            if (hide) { var h = document.querySelector(hide); if (h) h.classList.add('d-none'); }
        });
    });
})();
