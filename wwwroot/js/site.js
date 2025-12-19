document.addEventListener("DOMContentLoaded", function () {

    console.log("Register page JS loaded");

    const jobBtn = document.getElementById('jobSeekerBtn');
    const empBtn = document.getElementById('employerBtn');
    const jobForm = document.getElementById('jobSeekerForm');
    const empForm = document.getElementById('employerForm');

    if (jobBtn && empBtn && jobForm && empForm) {
        jobBtn.addEventListener('click', () => {
            jobBtn.classList.add('active');
            empBtn.classList.remove('active');
            jobForm.classList.remove('d-none');
            empForm.classList.add('d-none');
        });

        empBtn.addEventListener('click', () => {
            empBtn.classList.add('active');
            jobBtn.classList.remove('active');
            empForm.classList.remove('d-none');
            jobForm.classList.add('d-none');
        });
    }

    // LOGIN FORM
    const loginForm = document.getElementById("loginForm");

    if (loginForm) {
        loginForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const formData = new FormData(loginForm);
            let response;

            try {
                response = await fetch("/Accounts/Login", {
                    method: "POST",
                    body: formData
                });

                let result = {};
                const contentType = response.headers.get("content-type");

                if (contentType && contentType.includes("application/json")) {
                    result = await response.json();
                } else {
                    result = { message: "Unexpected response from server." };
                }

                if (response.status === 429) {
                    showError(result.message);
                    return;
                }

                if (!response.ok) {
                    showError(result.message || "Invalid email or password.");
                    return;
                }

                window.location.href = result.redirect;

            } catch (err) {
                showError("Network error. Please try again.");
            }
        });
    }

    function showError(message) {
        document.getElementById("errorModalBody").innerText = message;
        bootstrap.Modal
            .getOrCreateInstance(document.getElementById("errorModal"))
            .show();
    }

    document.addEventListener("hidden.bs.modal", () => {
        document.body.classList.remove("modal-open");

        const backdrop = document.querySelector(".modal-backdrop");
        if (backdrop) backdrop.remove();

        document.body.style.overflow = "auto";
    });

});
