document.addEventListener("DOMContentLoaded", () => {

    document.querySelectorAll(".apply-btn").forEach(btn => {
        btn.addEventListener("click", async () => {

            const jobId = btn.dataset.jobId;

            try {
                const response = await fetch(`/Jobs/ApplyCheck?jobId=${jobId}`);

                let result = {};
                const contentType = response.headers.get("content-type");

                if (!contentType || !contentType.includes("application/json")) {
                    showLoginModal();
                    return;
                }

                result = await response.json();

                if (!response.ok || !result.isJobSeeker) {
                    showLoginModal();
                    return;
                }

                const applyResponse = await fetch(`/Jobs/Apply?jobId=${jobId}`, {
                    method: "POST"
                });

                let applyResult = {};
                const applyType = applyResponse.headers.get("content-type");

                if (applyType && applyType.includes("application/json")) {
                    applyResult = await applyResponse.json();
                }

                if (applyResponse.ok && applyResult.success) {
                    showSuccess(applyResult.message);
                } else {
                    showError(applyResult.message || "Application failed.");
                }

            } catch {
                showError("Network error. Please try again.");
            }
        });
    });

    document.getElementById("applyFilters")?.addEventListener("click", () => {
        const category = document.getElementById("filterCategory").value;
        const jobType = document.getElementById("filterType").value;
        const location = document.getElementById("filterLocation").value;

        const url = new URL(window.location.origin + "/Jobs/Jobs");

        url.searchParams.set("category", category);
        url.searchParams.set("jobType", jobType);
        url.searchParams.set("location", location);

        window.location.href = url.toString();
    });

    function showLoginModal() {
        new bootstrap.Modal(
            document.getElementById("loginRequiredModal")
        ).show();
    }

    function showSuccess(msg) {
        document.getElementById("successModalBody").innerText = msg;
        new bootstrap.Modal(
            document.getElementById("successModal")
        ).show();
    }

    function showError(msg) {
        document.getElementById("errorModalBody").innerText = msg;
        new bootstrap.Modal(
            document.getElementById("errorModal")
        ).show();
    }
});
