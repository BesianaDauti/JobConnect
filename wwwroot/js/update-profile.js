document.addEventListener("DOMContentLoaded", () => {

    const form = document.getElementById("updateProfileForm");
    if (!form) return;

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = new FormData(form);

        let response;
        let result = {};

        try {
            response = await fetch("/JobSeeker/UpdateProfile", {
                method: "POST",
                body: formData
            });

            try {
                result = await response.json();
            } catch {
                result = { message: "Unexpected server response." };
            }

        } catch {
            result = { message: "Network error. Please try again." };
        }

        if (response && response.ok) {

            document.getElementById("successModalBody").innerText = result.message;
            bootstrap.Modal
                .getOrCreateInstance(document.getElementById("successModal"))
                .show();

            setTimeout(() => {
                window.location.href = result.redirect;
            }, 1500);

        } else {

            document.getElementById("errorModalBody").innerText =
                result.message || "Failed to update profile.";

            bootstrap.Modal
                .getOrCreateInstance(document.getElementById("errorModal"))
                .show();
        }
    });
});
