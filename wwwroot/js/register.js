document.addEventListener("DOMContentLoaded", () => {

    const jobForm = document.getElementById("jobSeekerForm");
    const employerForm = document.getElementById("employerForm");

    if (jobForm) {
        jobForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const formData = new FormData(jobForm);

            const response = await fetch("/Accounts/RegisterJobSeeker", {
                method: "POST",
                body: formData
            });

            const result = await response.json();

            if (response.ok) {
                document.getElementById("successModalBody").innerText = result.message;
                new bootstrap.Modal(document.getElementById("successModal")).show();

                if (result.redirect) {
                    setTimeout(() => {
                        window.location.href = result.redirect;
                    }, 3000);
                }
            } else {
                const errorModalEl = document.getElementById("errorModal");
                document.getElementById("errorModalBody").innerText =
                    result.message || "An error occurred.";

                const errorModal = new bootstrap.Modal(errorModalEl);
                errorModal.show();

                errorModalEl.addEventListener("hidden.bs.modal", () => {
                    document.querySelectorAll(".modal-backdrop").forEach(el => el.remove());
                    document.body.classList.remove("modal-open");
                });

            }
        });
    }

    if (employerForm) {
        employerForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const formData = new FormData(employerForm);

            const response = await fetch("/Accounts/RegisterEmployer", {
                method: "POST",
                body: formData
            });

            const result = await response.json();

            if (response.ok) {
                document.getElementById("successModalBody").innerText = result.message;
                new bootstrap.Modal(document.getElementById("successModal")).show();

                if (result.redirect) {
                    setTimeout(() => {
                        window.location.href = result.redirect;
                    }, 3000);
                }
            } else {
                const errorModalEl = document.getElementById("errorModal");
                document.getElementById("errorModalBody").innerText =
                    result.message || "An error occurred.";

                const errorModal = new bootstrap.Modal(errorModalEl);
                errorModal.show();

                errorModalEl.addEventListener("hidden.bs.modal", () => {
                    document.querySelectorAll(".modal-backdrop").forEach(el => el.remove());
                    document.body.classList.remove("modal-open");
                });

            }
        });
    }

});
