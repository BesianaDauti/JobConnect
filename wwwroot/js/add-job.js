document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("addJobForm");

    if (!form) return;
    if (form.dataset.bound) return;

    form.dataset.bound = "true";

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const btn = form.querySelector("button[type='submit']");
        btn.disabled = true;

        const formData = new FormData(form);

        const response = await fetch("/Employer/AddJob", {
            method: "POST",
            body: formData
        });

        let result = {};
        try {
            result = await response.json();
        } catch {
            result = { message: "Unexpected server response." };
        }

        btn.disabled = false;

        if (response.ok) {
            document.getElementById("successModalBody").innerText = result.message;
            bootstrap.Modal.getOrCreateInstance(
                document.getElementById("successModal")
            ).show();

            setTimeout(() => {
                window.location.href = result.redirect;
            }, 1500);
        } else {
            document.getElementById("errorModalBody").innerText = result.message;
            bootstrap.Modal.getOrCreateInstance(
                document.getElementById("errorModal")
            ).show();
        }
    });
});
